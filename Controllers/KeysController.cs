using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using tinyidp.Business.BusinessEntities;
using tinyidp.Controllers.Models;
using tinyidp.Extensions;
using tinyidp.infrastructure.keysmanagment;

namespace tinyidp.Controllers;

[ApiController]
[Route("[controller]")]
public class KeysController: Controller
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IKeysManagment _keyManagment;

    public KeysController(ILogger<KeysController> logger, IConfiguration configuration, IKeysManagment keysManagment)
    {
        _logger = logger;
        _configuration = configuration;
        _keyManagment = keysManagment;
    }

   [HttpGet("jwks.json")]
    public IActionResult Jwks()
    {
        List<KidBusinessEntity> kids = _keyManagment.GetActiveKeys();
        KeysResponse resp = new KeysResponse();
        resp.Keys = kids.Select(
            p => {
                JsonWebKey webKey;
                switch (p.Algo)
                {
                    case AlgoType.RSA:
                        webKey = JsonWebKeyConverter.ConvertFromRSASecurityKey(p.GetRsaSecurityKey());
                        break;
                    case AlgoType.ECC:
                        webKey = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(p.GetEccSecurityKey());
                        break;
                    default:
                        webKey = new JsonWebKey();
                        break;
                }
                
                webKey.KeyId = p.Kid1;
                return webKey;
            }
            ).ToList();
        return Ok(resp);
    }    
}