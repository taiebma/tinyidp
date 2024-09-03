using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using tinyidp.Business.Credential;
using tinyidp.Extensions;
using Microsoft.AspNetCore.Http;
using tinyidp.Business.BusinessEntities;
using tinyidp.Encryption;
using tinyidp.Exceptions;
using tinyidp.infrastructure.bdd;
using tinyidp.infrastructure.keysmanagment;

namespace tinyidp.Business.tokens;

public class RefreshToken : ITokenStrategy
{
    private readonly IConfiguration _conf;
    private readonly ILogger<RefreshToken> _logger;
    private readonly IKeysManagment _keysManagment;
    private readonly ITokenRepository _tokenRepository;
    private readonly ICredentialBusiness _credentialBusiness;
    private readonly IEncryptionService _encryptionService;

    public static TokenTypeEnum Type => TokenTypeEnum.refresh_token;

    public RefreshToken(
            IConfiguration conf, 
            ILogger<RefreshToken> logger, 
            IKeysManagment keysManagment,
            ITokenRepository tokenRepository,
            ICredentialBusiness credentialBusiness,
            IEncryptionService encryptionService)
    {
        _conf = conf;
        _logger = logger;
        _tokenRepository = tokenRepository;
        _credentialBusiness = credentialBusiness;
        _keysManagment = keysManagment;
        _encryptionService = encryptionService;
    }

    public async Task<TokenResponseBusiness> GetTokenByType(HttpContext httpContext, TokenRequestBusiness request, CredentialBusinessEntity client)
    {
        if (!await VerifyClientHeader(httpContext, request, client))
        {
            throw new TinyidpTokenException("Invalid credentials", "unauthorized_client");            
        }

        if (client.CreationDateRefreshToken == null)
        {
            throw new TinyidpTokenException("Token and creation date are inconsistent", "invalid_token");            
        }
        if (client.CreationDateRefreshToken.Value.AddMinutes(client.RefreshMaxMinuteValidity) < DateTime.Now)
        {
            throw new TinyidpTokenException("Refresh token expired", "invalid_token");
        }

        RefreshTokenResponse? refreshTokenResponse;
        try
        {
            refreshTokenResponse = JsonSerializer.Deserialize<RefreshTokenResponse>( _encryptionService.Decrypt( request.refresh_token??"" ));
            if (refreshTokenResponse == null)
                throw new TinyidpTokenException("Error when decoding refresh_token", "invalid_token");
        }
        catch (Exception ex)
        {
            throw new TinyidpTokenException("Error when decoding refresh_token", "invalid_token", ex);
        }

        TokenResponseBusiness resp = new TokenResponseBusiness();
        resp.access_token = await Task.Run( () => _keysManagment.GenerateJWTToken(
            refreshTokenResponse.Algo, refreshTokenResponse.Scopes, refreshTokenResponse.Audiences, refreshTokenResponse.Ident, refreshTokenResponse.LifeTime));

        resp.token_type = "Bearer";

        resp.refreshTokenResponse = refreshTokenResponse;

        return resp;
    }

    public async Task<bool> VerifyClientHeader(HttpContext httpContext, TokenRequestBusiness request, CredentialBusinessEntity client)
    {
        BasicIdent ident = httpContext.GetBasicIdent();

        if (ident.ClientId != client.Ident)
            throw new TinyidpTokenException("Client corresponding of the refresh_token is not the same than Authorization header", "invalid_request");
        
        return await _credentialBusiness.VerifyPassword(ident.ClientId, ident.ClientSecret);
    }
}