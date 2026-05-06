
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

namespace tinyidp.Business.keysmanagment;

public class KeysManagment : IKeysManagment
{
    private readonly IConfiguration _conf;
    private readonly ILogger<KeysManagment> _logger;
    private readonly IEncryptionService _encryptionService;
    private readonly IKidRepository _kidRepository;
    private readonly IMemoryCache _memoryCache;
    
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
        
    }

    public async Task<List<KidBusinessEntity>> GetKeys()
    {
        List<KidBusinessEntity>? keys;
        if (!_memoryCache.TryGetValue("keys", out keys))
        {
            List<Kid> kids =  await _kidRepository.GetAll();
            if (kids.Count == 0)
            {
                keys = new List<KidBusinessEntity>();
            }
            else
            {
                keys = kids.Select(p => p.ToBusiness(_encryptionService)).ToList();
            }
            _memoryCache.Set("keys", keys, DateTime.Now.AddHours(1));
        }
        return keys??new List<KidBusinessEntity>();
    }

    public async Task<List<KidBusinessEntity>> GetActiveKeys()
    {
        var keys = await GetKeys();
        return keys.Where(p => p.State == KidState.Active && p.Valid == true).ToList()??new List<KidBusinessEntity>();
    }

    public async Task<KidBusinessEntity> GenNewKey(AlgoType algo, string kid)
    {
        KidBusinessEntity key;

        switch (algo )
        {
            case AlgoType.RSA:
                key = await GenNewRSAKey(kid);
                break;
            case AlgoType.ECC:
                key = await GenNewECCKey(kid);
                break;
            default:
                throw new TinyidpKeyException("Algorithm unknown");
        }
        return key;
    }

    public async Task<KidBusinessEntity> GenNewRSAKey(string kid)
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

        var keys = await GetKeys();
        if (keys != null)
        {
            newkid.Id = kidEntity.Id;
            newkid.Valid = true;
            keys.Add(newkid);
            _memoryCache.Set("keys", keys, DateTime.Now.AddMinutes(5));
        }

        return newkid;
    }

    public async Task<KidBusinessEntity> GenNewECCKey(string kid)
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

        var keys = await GetKeys();
        if (keys != null)
        {
            newkid.Id = kidEntity.Id;
            newkid.Valid = true;
            keys.Add(newkid);
            _memoryCache.Set("keys", keys, DateTime.Now.AddMinutes(5));
        }

        return newkid;
    }

    public async Task<KidBusinessEntity?> GetKeyByKid(string kid)
    {
        var keys = await GetKeys();
        return keys.Where(p => p.Kid1 == kid).FirstOrDefault();
    }

    public async Task<KidBusinessEntity?> GetKeyById(int id)
    {
        var keys = await GetKeys();
        return keys.Where(p => p.Id == id).FirstOrDefault();
    }

    public async Task<KidBusinessEntity?> LastActive(AlgoType algo)
    {
        var keys = await GetKeys();
        if (keys == null)
        {
            throw new Exception("No keys");
        }
        return keys.LastOrDefault(p => p.Algo == algo 
            && p.State == KidState.Active
            && p.Valid);
    }

    public async Task Update(KidBusinessEntity kid)
    {
        var keys = await GetKeys();
        if (keys == null)
        {
            return;
        }
        KidBusinessEntity curKid = keys.First(p => p.Id == kid.Id);
        curKid.State = kid.State;
        _kidRepository.Update(curKid.ToEntity(_encryptionService));
        _memoryCache.Set("keys", keys, DateTime.Now.AddMinutes(5));
    }

    public async Task Remove(KidBusinessEntity kid)
    {
        var keys = await GetKeys();
        if (keys == null)
        {
            return;
        }
        _kidRepository.Remove(kid.ToEntity(_encryptionService));
        keys.Remove(keys.Where(p => p.Id == kid.Id).First());
        _memoryCache.Set("keys", keys, DateTime.Now.AddMinutes(5));
    }

    public async Task<string> GenerateJWTToken(AlgoKeyType keyType, IEnumerable<string> scopes, IEnumerable<string> audience, string? sub, long lifeTime, string? nonce)
    {
        string issuer = _conf["TINYIDP_IDP:BASE_URL_IDP"]??"https://localhost:7034/";
        var claims = new List<Claim>
        {
            new Claim("scope", string.Join(' ', scopes))
        };

        claims.Add(new Claim("iss", issuer));

        if (!string.IsNullOrEmpty(sub))
        {
            claims.Add(new Claim("sub", sub, ClaimValueTypes.String));
            claims.Add(new Claim("name", sub, ClaimValueTypes.String));
        }
        
        if (!string.IsNullOrEmpty(nonce))
        {
            claims.Add(new Claim("nonce", nonce, ClaimValueTypes.String));
        }
        
        foreach(string aud in audience)
        {
            claims.Add(new Claim("aud", aud));
        }

        RSACryptoServiceProvider provider1 = new RSACryptoServiceProvider();

        KidBusinessEntity? lastKid = await LastActive(keyType.ToAlgoType());
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

        DateTime tokenDate = DateTime.UtcNow;
        DateTimeOffset tokenDateOffset = new DateTimeOffset(tokenDate);
        claims.Add(new Claim("iat", tokenDateOffset.ToUnixTimeSeconds().ToString()));
        var token1 = new JwtSecurityToken(
            issuer,
            null, 
            claims, 
            notBefore: tokenDate,
            expires: DateTime.UtcNow.AddMinutes(lifeTime), 
            signingCredentials: signingCredentials
            );
        token1.Header.Add("kid", lastKid.Kid1);
        string access_token = handler.WriteToken(token1);

        return access_token;
    }

}