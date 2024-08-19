
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

namespace tinyidp.infrastructure.keysmanagment;

public class KeysManagment : IKeysManagment
{
    private readonly IConfiguration _conf;
    private readonly ILogger<KeysManagment> _logger;
    private readonly IEncryptionService _encryptionService;
    private readonly IKidRepository _kidRepository;
    
    private List<KidBusinessEntity> _listKeys;

    public KeysManagment(
        IConfiguration conf, 
        ILogger<KeysManagment> logger, 
        IEncryptionService encryptionService,
        IKidRepository repoKid)
    {
        _conf = conf;
        _logger = logger;
        _encryptionService = encryptionService;
        _kidRepository = repoKid;

        Task<List<Kid>> kids =  _kidRepository.GetAll();
        if (kids.Result.Count == 0)
        {
            _listKeys = new List<KidBusinessEntity>();
        }
        else
        {
            _listKeys = kids.Result.Select(p => p.ToBusiness(_encryptionService)).ToList();
        }
    }

    public List<KidBusinessEntity> GetKeys()
    {
        return _listKeys;
    }

    public List<KidBusinessEntity> GetActiveKeys()
    {
        return _listKeys.Where(p => p.State == KidState.Active && p.Valid == true).ToList();
    }

    public KidBusinessEntity GenNewKey(AlgoType algo, string kid)
    {
        using var rsa = RSA.Create(2048);
        KidBusinessEntity newkid = new KidBusinessEntity();
        newkid.Algo = algo;
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
            _listKeys.Add(newkid);
        }

        return newkid;
    }

    public KidBusinessEntity? GetKeyByKid(string kid)
    {
        return _listKeys?.Where(p => p.Kid1  == kid).First();
    }

    public KidBusinessEntity? GetKeyById(int id)
    {
        return _listKeys?.Where(p => p.Id  == id).First();
    }

    public KidBusinessEntity LastActive(AlgoType algo)
    {
        if (_listKeys == null)
        {
            throw new Exception("No keys");
        }
        return _listKeys.Last(p => p.Algo == algo 
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
    }

    public void Remove(KidBusinessEntity kid)
    {
        if (_listKeys == null)
        {
            return;
        }
        _kidRepository.Remove(kid.ToEntity(_encryptionService));
    }

    public string GenerateJWTToken(IEnumerable<string> scopes, IEnumerable<string> audience, string? sub)
    {
        string issuer = _conf.GetSection("TINYIDP_IDP").GetValue<string>("BASE_URL_IDP")??"https://localhost:7034/";
        int tokenLifetime = _conf.GetSection("TINYIDP_IDP").GetValue<int>("TOKEN_LIFETIME");
        var claims = new List<Claim>
        {
            new Claim("scope", string.Join(' ', scopes))
        };

        claims.Add(new Claim("iss", issuer));

        if (!string.IsNullOrEmpty(sub))
        {
            claims.Add(new Claim("sub", sub, ClaimValueTypes.String));
        }

        RSACryptoServiceProvider provider1 = new RSACryptoServiceProvider();

        KidBusinessEntity lastKid = LastActive(AlgoType.RSA);
        RsaSecurityKey rsaSecurityKey = lastKid.GetRsaPrivateSecurityKey();
        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

        var token1 = new JwtSecurityToken(
            issuer,
            JsonSerializer.Serialize(audience), 
            claims, 
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(tokenLifetime), 
            signingCredentials: new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256)
            );
        token1.Header.Add("kid", lastKid.Kid1);
        string access_token = handler.WriteToken(token1);

        return access_token;
    }

}