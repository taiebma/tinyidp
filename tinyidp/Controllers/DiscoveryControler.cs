
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.AspNetCore.Mvc;
using tinyidp.Business.tokens;
using tinyidp.Controllers.Models;

namespace tinyidp.Controllers;

[Route("oauth")]
[Produces("application/json")]
public class DiscoveryController: Controller
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public DiscoveryController(ILogger<DiscoveryController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet(".well-known/openid-configuration")]
    public IActionResult GetConfiguration()
    {
        string base_idp_url = _configuration.GetSection("TINYIDP_IDP").GetValue<string>("BASE_URL_IDP")??"https://localhost:7034/";
        var response = new DiscoveryResponse
        {
            issuer = base_idp_url,
            authorization_endpoint = base_idp_url + "/authorize",
            token_endpoint = base_idp_url + "/token",
            token_endpoint_auth_methods_supported = new string[] { "client_secret_basic" }, //, "private_key_jwt" },
            token_endpoint_auth_signing_alg_values_supported = new string[] { "RS256", "ES256" },
            grant_types_supported = new string[] { "authorization_code", "client_credentials"},
            response_types_supported = new string[] { "code", "token", "id_token"}, //"code id_token", "id_token", "token id_token" },
            subject_types_supported = new string[] { "public", "pairwise" },
            userinfo_endpoint = base_idp_url + "/userinfo",
            id_token_signing_alg_values_supported = new string[] { "RS256", "ES256" }, //, "HS256" , "SHA256" },
            request_object_signing_alg_values_supported = new string[] { "RS256", "ES256" },
            display_values_supported = new string[] { "page", "popup" },
            claim_types_supported = new string[] { "normal", "distributed" },
            jwks_uri = base_idp_url + "/keys/jwks.json",
            scopes_supported = TokenService.SupportedScopes,
            claims_supported = new string[] { "sub", "iss", "auth_time", "acr", "name", "given_name",
                "family_name", "nickname", "profile", "picture", "website", "email", "email_verified",
                "locale", "zoneinfo" },
            claims_parameter_supported = true,
            ui_locales_supported = new string[] { "en-US", "en-GB", "en-CA", "fr-FR", "fr-CA" }
            //acr_values_supported = new string[] {}, //"urn:mace:incommon:iap:silver", "urn:mace:incommon:iap:bronze"},
            //userinfo_encryption_enc_values_supported = new string[] { }, // "A128CBC-HS256", "A128GCM" },
            //id_token_encryption_alg_values_supported = new string[] { }, // "RSA1_5", "A128KW" },
            //id_token_encryption_enc_values_supported = new string[] { }, //"A128CBC-HS256", "A128GCM" },

        };

        return Json(response);
    }

}