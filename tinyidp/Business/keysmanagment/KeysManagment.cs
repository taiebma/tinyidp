
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using tinyidp.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using tinyidp.Business.BusinessEntities;
using tinyidp.Encryption;
using tinyidp.infrastructure.bdd;
using tinyidp.Exceptions;
using System.Text.Json.Serialization;

namespace tinyidp.infrastructure.keysmanagment;

public class KeysManagment : IKeysManagment
{
    private readonly IConfiguration _conf;
    private readonly ILogger<KeysManagment> _logger;
    private readonly IEncryptionService _encryptionService;
    private readonly IKidRepository _kidRepository;
    private readonly IMemoryCache _memoryCache;
    
    private List<KidBusinessEntity>? _listKeys;

    public KeysManagment(
        IConfiguration conf, 
        ILogger<KeysManagment> logger, 
        IEncryptionService encryptionService,
        IKidRepository repoKid,
        IMemoryCache memCache)
    {
        _conf = conf;
        _logger = logger;
        _encryptionService = encryptionService;
        _kidRepository = repoKid;
        _memoryCache = memCache;
        _listKeys = new List<KidBusinessEntity>();

        if (!_memoryCache.TryGetValue("keys", out _listKeys))
        {
            Task<List<Kid>> kids =  _kidRepository.GetAll();
            if (kids.Result.Count == 0)
            {
                _listKeys = new List<KidBusinessEntity>();
            }
            else
            {
                _listKeys = kids.Result.Select(p => p.ToBusiness(_encryptionService)).ToList();
            }
            _memoryCache.Set("keys", _listKeys, DateTime.Now.AddMinutes(5));
        }
        
    }

    public List<KidBusinessEntity> GetKeys()
    {
        return _listKeys??new List<KidBusinessEntity>();
    }

    public List<KidBusinessEntity> GetActiveKeys()
    {
        return _listKeys?.Where(p => p.State == KidState.Active && p.Valid == true).ToList()??new List<KidBusinessEntity>();
    }

    public KidBusinessEntity GenNewKey(AlgoType algo, string kid)
    {
        KidBusinessEntity key;

        switch (algo )
        {
            case AlgoType.RSA:
                key = GenNewRSAKey(kid);
                break;
            case AlgoType.ECC:
                key = GenNewECCKey(kid);
                break;
            default:
                throw new TinyidpKeyException("Algorithm unknown");
        }
        return key;
    }

    public KidBusinessEntity GenNewRSAKey(string kid)
    {
        using var rsa = RSA.Create(2048);
        KidBusinessEntity newkid = new KidBusinessEntity();
        newkid.Algo = AlgoType.RSA;
        newkid.CreationDate = DateTime.Now;
        newkid.State = KidState.Inactive;
        newkid.PrivateKey = rsa.ExportRSAPrivateKeyPem();
        newkid.PublicKey = rsa.ExportRSAPublicKeyPem();
        if (String.IsNullOrEmpty(kid))
        {
            newkid.Kid1 = newkid.PublicKey.GetHashString();
        }
        else
        {
            newkid.Kid1 = kid;
        }

        Kid kidEntity = newkid.ToEntity(_encryptionService);
        _kidRepository.Add(kidEntity);

        if (_listKeys == null)
        {
            GetKeys();
        }
        else
        {
            newkid.Id = kidEntity.Id;
            newkid.Valid = true;
            _listKeys.Add(newkid);
            _memoryCache.Set("keys", _listKeys, DateTime.Now.AddMinutes(5));
        }

        return newkid;
    }

    public KidBusinessEntity GenNewECCKey(string kid)
    {
        using var ecc = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        KidBusinessEntity newkid = new KidBusinessEntity();
        newkid.Algo = AlgoType.ECC;
        newkid.CreationDate = DateTime.Now;
        newkid.State = KidState.Inactive;
        newkid.PrivateKey = ecc.ExportECPrivateKeyPem();
        newkid.PublicKey = ecc.ExportSubjectPublicKeyInfoPem();
        if (String.IsNullOrEmpty(kid))
        {
            newkid.Kid1 = newkid.PublicKey.GetHashString();
        }
        else
        {
            newkid.Kid1 = kid;
        }

        Kid kidEntity = newkid.ToEntity(_encryptionService);
        _kidRepository.Add(kidEntity);

        if (_listKeys == null)
        {
            GetKeys();
        }
        else
        {
            newkid.Id = kidEntity.Id;
            newkid.Valid = true;
            _listKeys.Add(newkid);
            _memoryCache.Set("keys", _listKeys, DateTime.Now.AddMinutes(5));
        }

        return newkid;
    }

    public KidBusinessEntity? GetKeyByKid(string kid)
    {
        return _listKeys?.Where(p => p.Kid1  == kid).FirstOrDefault();
    }

    public KidBusinessEntity? GetKeyById(int id)
    {
        return _listKeys?.Where(p => p.Id  == id).FirstOrDefault();
    }

    public KidBusinessEntity? LastActive(AlgoType algo)
    {
        if (_listKeys == null)
        {
            throw new Exception("No keys");
        }
        return _listKeys.LastOrDefault(p => p.Algo == algo 
            && p.State == KidState.Active
            && p.Valid);
    }

    public void Update(KidBusinessEntity kid)
    {
        if (_listKeys == null)
        {
            return;
        }
        KidBusinessEntity curKid = _listKeys.First(p => p.Id == kid.Id);
        curKid.State = kid.State;
        _kidRepository.Update(curKid.ToEntity(_encryptionService));
        _memoryCache.Set("keys", _listKeys, DateTime.Now.AddMinutes(5));
    }

    public void Remove(KidBusinessEntity kid)
    {
        if (_listKeys == null)
        {
            return;
        }
        _kidRepository.Remove(kid.ToEntity(_encryptionService));
        _listKeys.Remove(_listKeys.Where(p => p.Id == kid.Id).First());
        _memoryCache.Set("keys", _listKeys, DateTime.Now.AddMinutes(5));
    }

    public string GenerateJWTToken(AlgoKeyType keyType, IEnumerable<string> scopes, IEnumerable<string> audience, string? sub, long lifeTime)
    {
        string issuer = _conf.GetSection("TINYIDP_IDP")?.GetValue<string>("BASE_URL_IDP")??"https://localhost:7034/";
        var claims = new List<Claim>
        {
            new Claim("scope", string.Join(' ', scopes))
        };

        claims.Add(new Claim("iss", issuer));

        if (!string.IsNullOrEmpty(sub))
        {
            claims.Add(new Claim("sub", sub, ClaimValueTypes.String));
        }
        
        foreach(string aud in audience)
        {
            claims.Add(new Claim("aud", aud));
        }

        RSACryptoServiceProvider provider1 = new RSACryptoServiceProvider();

        KidBusinessEntity? lastKid = LastActive(keyType.ToAlgoType());
        SigningCredentials signingCredentials;

        if (lastKid == null)
            throw new TinyidpKeyException("No available key found for requested algo");

        switch (lastKid.Algo)
        {
            case AlgoType.RSA:
                RsaSecurityKey rsaSecurityKey = lastKid.GetRsaPrivateSecurityKey();
                signingCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);
                break;
            case AlgoType.ECC:
                ECDsaSecurityKey eccSecurityKey = lastKid.GetEccPrivateSecurityKey();
                signingCredentials = new SigningCredentials(eccSecurityKey, SecurityAlgorithms.EcdsaSha256);
                break;
            default:
                throw new TinyidpKeyException("Algo unknown for signing token");
        }
        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

        var token1 = new JwtSecurityToken(
            issuer,
            null, 
            claims, 
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(lifeTime), 
            signingCredentials: signingCredentials
            );
        token1.Header.Add("kid", lastKid.Kid1);
        string access_token = handler.WriteToken(token1);

        return access_token;
    }

}