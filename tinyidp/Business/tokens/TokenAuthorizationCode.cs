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

public class TokenAuthorizationCode : ITokenStrategy
{
    private readonly IConfiguration _conf;
    private readonly ILogger<TokenAuthorizationCode> _logger;
    private readonly IKeysManagment _keysManagment;
    private readonly IPKCEService _pkceService;
    private readonly ITokenRepository _tokenRepository;
    private readonly ICredentialBusiness _credentialBusiness;

    public static TokenTypeEnum Type => TokenTypeEnum.code;

    public TokenAuthorizationCode(
            IConfiguration conf, 
            ILogger<TokenAuthorizationCode> logger, 
            IKeysManagment keysManagment,
            ITokenRepository tokenRepository,
            ICredentialBusiness credentialBusiness,
            IPKCEService pkceService)
    {
        _conf = conf;
        _logger = logger;
        _tokenRepository = tokenRepository;
        _credentialBusiness = credentialBusiness;
        _keysManagment = keysManagment;
        _pkceService = pkceService;
    }

    public TokenResponseBusiness GetTokenByType(TokenRequestBusiness request, CredentialBusinessEntity client)
    {
        IEnumerable<string> scopes = new List<string>();
        if (client.AllowedScopes != null)
        {
            scopes = client.AllowedScopes.Intersect(request.scope??new List<string>());
            if (!scopes.Any())
            {
                throw new TinyidpTokenException("No scope match", "invalid_scope");
            }
        }

        if (!String.IsNullOrEmpty(client.CodeChallenge) && string.IsNullOrEmpty(request.code_verifier))
            throw new TinyidpTokenException("Client need code verifier", "invalid_request");

        if (request.code_verifier != null)
        {
            if (client.CodeChallengeMethod?.Equals("S256")??false)
            {
                if (!_pkceService.ValidPKCE(request.code_verifier, client.CodeChallenge??String.Empty))
                    throw new TinyidpTokenException("Challenge PKCE not corresponding");
            }
            else if (client.CodeChallengeMethod?.Equals("plain")??false)
            {
                if (!request.code_verifier.Equals(client.CodeChallenge))
                    throw new TinyidpTokenException("Challenge PKCE not corresponding");
            }
        }

        if (String.IsNullOrEmpty(request.code))
            throw new TinyidpTokenException("No authorization code", "invalid_request");

        CredentialBusinessEntity? user = _credentialBusiness.GetByAuthorizationCode(request.code).Result;
        if (user == null)
        {
            throw new TinyidpTokenException("Authorization code not found");
        }

        TokenResponseBusiness resp = new TokenResponseBusiness();
        resp.access_token = _keysManagment.GenerateJWTToken(
            client.KeyType, scopes, client.Audiences??new List<string>(), user.Ident, client.TokenMaxMinuteValidity);

        resp.token_type = "Bearer";

        // Save informations for refresh token
        RefreshTokenResponse tokenResp = new RefreshTokenResponse()
        {
            Scopes = scopes, 
            Audiences = client.Audiences??new List<string>(), 
            Ident = user.Ident,
            Algo = client.KeyType, 
            LifeTime = client.TokenMaxMinuteValidity
        };
        resp.refreshTokenResponse = tokenResp;

        // Authorization code can be use only one time
        user.AuthorizationCode = "";
        _credentialBusiness.Update(user);

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
