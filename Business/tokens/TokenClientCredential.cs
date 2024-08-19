using System.Text;
using tinyidp.Business.Credential;
using tinyidp.Extensions;
using Microsoft.AspNetCore.Http;
using tinyidp.Business.BusinessEntities;
using tinyidp.Encryption;
using tinyidp.Exceptions;
using tinyidp.infrastructure.bdd;
using tinyidp.infrastructure.keysmanagment;

namespace tinyidp.Business.tokens;

public class TokenClientCredential : ITokenStrategy
{
    private readonly IConfiguration _conf;
    private readonly ILogger<TokenClientCredential> _logger;
    private readonly IKeysManagment _keysManagment;
    private readonly ITokenRepository _tokenRepository;
    private readonly ICredentialBusiness _credentialBusiness;

    public static TokenTypeEnum Type => TokenTypeEnum.client_credential;

    public TokenClientCredential(
            IConfiguration conf, 
            ILogger<TokenClientCredential> logger, 
            IKeysManagment keysManagment,
            ITokenRepository tokenRepository,
            ICredentialBusiness credentialBusiness)
    {
        _conf = conf;
        _logger = logger;
        _tokenRepository = tokenRepository;
        _credentialBusiness = credentialBusiness;
        _keysManagment = keysManagment;
    }

    public async Task<TokenResponseBusiness> GetTokenByType(HttpContext httpContext, TokenRequestBusiness request, CredentialBusinessEntity client)
    {
        if (!await VerifyClientHeader(httpContext, request, client))
        {
            throw new TinyidpTokenException("Invalid credentials", "unauthorized_client");            
        }

        IEnumerable<string> scopes = new List<string>();
        if (client.AllowedScopes != null)
        {
            scopes = client.AllowedScopes.Intersect(request.scope??new List<string>());
            if (!scopes.Any())
            {
                throw new TinyidpTokenException("No scope match", "invalid_scope");
            }
        }
        TokenResponseBusiness resp = new TokenResponseBusiness();
        resp.access_token = _keysManagment.GenerateJWTToken(
            scopes, client.Audiences??new List<string>(), null);

        resp.token_type = "Bearer";

        // Save informations for refresh token
        RefreshTokenResponse tokenResp = new RefreshTokenResponse()
        {
            Scopes = scopes, 
            Audiences = client.Audiences??new List<string>(), 
            Ident = null
        };
        resp.refreshTokenResponse = tokenResp;

        return resp;
    }

    public async Task<bool> VerifyClientHeader(HttpContext httpContext, TokenRequestBusiness request, CredentialBusinessEntity client)
    {
        if (client.RoleIdent != RoleCredential.Client)
            throw new TinyidpTokenException("Only client role can use client_credential", "unsupported_grant_type");

        var authHeader = httpContext.Request.Headers["Authorization"].ToString();
        if (authHeader == null)
            throw new TinyidpTokenException("No Authorization header", "invalid_request");

        if (!authHeader.StartsWith("Basic", StringComparison.OrdinalIgnoreCase))
            throw new TinyidpTokenException("For client_credential grant_type, Authorization must be Basic ", "invalid_request");

        var parameters = authHeader.Substring("Basic ".Length);
        var authorizationKeys = Encoding.UTF8.GetString(Convert.FromBase64String(parameters));

        var authorizationResult = authorizationKeys.IndexOf(':');
        if (authorizationResult == -1)
            throw new TinyidpTokenException("Basic Authorization must be <client_id>:<client_secret> format", "invalid_request");

        string clientId = authorizationKeys.Substring(0, authorizationResult);
        string clientSecret = authorizationKeys.Substring(authorizationResult + 1);

        if (clientId != request.client_id)
            throw new TinyidpTokenException("Client_id of the request is not the same than Authorization header", "invalid_request");
        
        if (clientSecret != Encoding.UTF8.GetString(Convert.FromBase64String(request.client_secret??String.Empty)))
            throw new TinyidpTokenException("client_secret of the request is not the same than Authorization header", "invalid_request");
        
        return await _credentialBusiness.VerifyPassword(clientId, clientSecret);
    }
}