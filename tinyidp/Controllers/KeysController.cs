using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using tinyidp.Business.BusinessEntities;
using tinyidp.Controllers.Models;
using tinyidp.Extensions;
using tinyidp.Business.keysmanagment;

namespace tinyidp.Controllers;

public class KeysController
{

    public static async Task<IResult> Jwks([FromServices] IKeysManagment _keyManagment)
    {
        List<KidBusinessEntity> kids = await _keyManagment.GetActiveKeys();
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
        return Results.Ok(resp);
    }    
}