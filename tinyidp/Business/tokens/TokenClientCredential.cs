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

    public TokenResponseBusiness GetTokenByType(TokenRequestBusiness request, CredentialBusinessEntity client)
    {
        IEnumerable<string> scopes = new List<string>();
        if (client.AllowedScopes != null && request.scope != null)
        {
            var allClientScopes = client.AllowedScopes.Concat(TokenService.SupportedScopes);
            if (request.scope.Where(p => !allClientScopes.Contains(p)).ToList().Count() > 0)
            {
                throw new TinyidpTokenException("No scope match", "invalid_scope");
            }
            scopes = allClientScopes.Intersect(request.scope??new List<string>());
        }

        TokenResponseBusiness resp = new TokenResponseBusiness();
        resp.access_token = _keysManagment.GenerateJWTToken(
            client.KeyType, scopes, client.Audiences??new List<string>(), null, client.TokenMaxMinuteValidity, null);
        resp.id_token = resp.access_token;

        resp.token_type = "Bearer";

        // Save informations for refresh token
        RefreshTokenResponse tokenResp = new RefreshTokenResponse()
        {
            Scopes = scopes, 
            Audiences = client.Audiences??new List<string>(), 
            Ident = null, 
            Algo = client.KeyType, 
            LifeTime = client.TokenMaxMinuteValidity
        };
        resp.refreshTokenResponse = tokenResp;

        return resp;
    }

    public bool VerifyClientIdent(BasicIdent ident, TokenRequestBusiness request, CredentialBusinessEntity client, bool checkPwd)
    {
        if (client.RoleIdent != RoleCredential.Client)
            throw new TinyidpTokenException("Only client role can use client_credential", "unsupported_grant_type");

        if (ident.ClientId != request.client_id)
            throw new TinyidpTokenException("Client_id of the request is not the same than Authorization header", "invalid_request");
        
        if (!checkPwd)
            return true;
        
        string secretConverted;
        try
        {
            secretConverted = Encoding.UTF8.GetString(Convert.FromBase64String(request.client_secret??String.Empty));
        }
        catch (FormatException)
        {
            throw new TinyidpTokenException("Bad secret format", "invalid_request");
        }
        if (ident.ClientSecret != secretConverted)
            throw new TinyidpTokenException("client_secret of the request is not the same than Authorization header", "invalid_request");
        
        return _credentialBusiness.CheckPassword(client.Pass, ident.ClientSecret);
    }

}