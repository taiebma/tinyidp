
using System.Web;
using System.Net.Http.Headers;
using tinyidp.Business.Credential;
using tinyidp.Extensions;
using Microsoft.AspNetCore.Mvc;
using tinyidp.Business.BusinessEntities;
using tinyidp.Business.tokens;
using tinyidp.Controllers.Models;
using tinyidp.Exceptions;
using Microsoft.Net.Http.Headers;

namespace tinyidp.Controllers;

[ApiController]
[Route("oauth")]
public class OAuthController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly ITokenService _tokenService;
    private readonly ICredentialBusiness _credentialBusiness;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OAuthController(ILogger<OAuthController> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ITokenService tokenService, ICredentialBusiness credentialBusiness)
    {
        _logger = logger;
        _configuration = configuration;
        _tokenService = tokenService;
        _httpContextAccessor = httpContextAccessor;
        _credentialBusiness = credentialBusiness;
    }

    [HttpPost("token")]
    public async Task<IActionResult> GetToken([FromForm]TokenRequest request)
    {
        TokenResponse resp;

        try 
        {
            var respBuis = await _tokenService.GetToken(_httpContextAccessor.HttpContext, request.ToBusiness());
            resp = respBuis.ToModel();
        }
        catch(TinyidpTokenException ex)
        {
            resp = new TokenResponse();
            resp.Error = ex.Message;
            resp.error_description = ex.error_description;
            return BadRequest(resp);
        }
        catch(TinyidpKeyException ex)
        {
            resp = new TokenResponse();
            resp.Error = ex.Message;
            resp.error_description = "";
            return BadRequest(resp);
        }
        return Ok(resp);
    }

    [HttpGet("authorize")]
    public async Task<IActionResult> Authorize([FromQuery]tinyidp.Controllers.Models.AuthorizationRequest request)
    {
        
        if (!(_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated??false) &&
            _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].Count == 0)
        {
            return Redirect(String.Format("/Account/Ident?scope={0}&state={1}&nonce={2}&redirect_uri={3}&client_id={4}&code_challenge={5}&code_challenge_method={6}", 
                request.scope, 
                request.state, 
                request.nonce,
                HttpUtility.UrlEncode(request.redirect_uri),
                request.client_id,
                request.code_challenge,
                request.code_challenge_method
                ));
        }
        
        CredentialBusinessEntity client;
        try
        {
            client = await _credentialBusiness.Authorize(_httpContextAccessor.HttpContext, request.ToBusiness());
        }
        catch(TinyidpCredentialException ex)
        {
            return BadRequest(ex.Message);
        }


        return Redirect(String.Format("{0}?code={1}&state={2}&nonce={3}", 
            client.RedirectUri,
            client.AuthorizationCode,
            request.state,
            request.nonce));

    }
}