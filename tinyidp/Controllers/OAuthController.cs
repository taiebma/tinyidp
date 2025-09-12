
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

public class OAuthController 
{

    public static async Task<IResult> GetToken([FromForm]TokenRequest request, [FromServices] ITokenService _tokenService, [FromServices] IHttpContextAccessor _httpContextAccessor)
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
            return Results.BadRequest(resp);
        }
        catch(TinyidpKeyException ex)
        {
            resp = new TokenResponse();
            resp.Error = ex.Message;
            resp.error_description = "";
            return Results.BadRequest(resp);
        }
        catch(TinyidpCertificateException ex)
        {
            resp = new TokenResponse();
            resp.Error = ex.Message;
            resp.error_description = "";
            return Results.BadRequest(resp);
        }
        return Results.Ok(resp);
    }

    public static async Task<IResult> Authorize(
        [FromQuery]string response_type,
        [FromQuery]string client_id,
        [FromQuery]string redirect_uri,
        [FromQuery]string scope,
        [FromQuery]string state,
        [FromQuery]string? nonce,
        [FromQuery]string? code_challenge,
        [FromQuery]string? code_challenge_method,
        [FromServices] IHttpContextAccessor _httpContextAccessor, [FromServices] ICredentialBusiness _credentialBusiness)
    {
        var request = new tinyidp.Controllers.Models.AuthorizationRequest
        {
            response_type = response_type,
            client_id = client_id,
            redirect_uri = redirect_uri,
            scope = scope,
            state = state,
            nonce = nonce,
            code_challenge = code_challenge,
            code_challenge_method = code_challenge_method
        };

        
        if (!(_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated??false) &&
            _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].Count == 0)
        {
            return Results.Redirect(String.Format("/Account/Ident?scope={0}&state={1}&nonce={2}&redirect_uri={3}&client_id={4}&code_challenge={5}&code_challenge_method={6}&nonce={7}", 
                request.scope, 
                request.state, 
                request.nonce,
                HttpUtility.UrlEncode(request.redirect_uri),
                request.client_id,
                request.code_challenge,
                request.code_challenge_method,
                request.nonce
                ));
        }
        
        CredentialBusinessEntity client;
        try
        {
            client = await _credentialBusiness.Authorize(_httpContextAccessor.HttpContext, request.ToBusiness());
        }
        catch(TinyidpCredentialException ex)
        {
            return Results.BadRequest(ex.Message);
        }


        return Results.Redirect(String.Format("{0}?code={1}&state={2}&nonce={3}&scope={4}", 
            client.RedirectUri,
            client.AuthorizationCode,
            request.state,
            request.nonce,
            request.scope));

    }

    public static IResult UserInfo([FromServices] IHttpContextAccessor _httpContextAccessor, [FromServices] ICredentialBusiness _credentialBusiness)
    {
        
        if (!(_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated??false) &&
            _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].Count == 0)
        {
            return Results.Unauthorized();
        }
        
        try
        {
            Business.BusinessEntities.AppUser user = _credentialBusiness.GetUserInfo(_httpContextAccessor.HttpContext);
            return Results.Ok(user);
        }
        catch(TinyidpCredentialException )
        {
            return Results.Unauthorized();
        }

    }
}