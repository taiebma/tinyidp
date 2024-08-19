using System.Security.Cryptography;
using System.Text.Json;
using tinyidp.Business.Credential;
using Microsoft.AspNetCore.Http;
using tinyidp.Business.BusinessEntities;
using tinyidp.Encryption;
using tinyidp.Exceptions;
using tinyidp.infrastructure.bdd;

namespace tinyidp.Business.tokens;

public class TokenService : ITokenService
{
    private readonly IConfiguration _conf;
    private readonly ILogger<TokenService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEncryptionService _encryptionService;
    private readonly ITokenRepository _tokenRepository;
    private readonly ICredentialBusiness _credentialBusiness;

    public TokenService(
            IConfiguration conf, 
            ILogger<TokenService> logger, 
            IServiceProvider serviceProvider,
            IEncryptionService encryptionService,
            ITokenRepository tokenRepository,
            ICredentialBusiness credentialBusiness)
    {
        _conf = conf;
        _logger = logger;
        _tokenRepository = tokenRepository;
        _credentialBusiness = credentialBusiness;
        _encryptionService = encryptionService;
        _serviceProvider = serviceProvider;
    }

    public async Task<TokenResponseBusiness> GetToken(HttpContext? httpContext, TokenRequestBusiness request)
    {
        CredentialBusinessEntity? client;

        if (httpContext == null)
        {
            throw new TinyidpTokenException("No HTTP Context");
        }
        if (request.grant_type == "refresh_token")
        {
            if (!String.IsNullOrEmpty(request.refresh_token))
                client = await _credentialBusiness.GetByRefreshToken(request.refresh_token);
            else
                throw new TinyidpTokenException("No refresh token", "invalid_request");
        }
        else
        {
            client = await _credentialBusiness.GetByIdent(request.client_id??String.Empty);
        }
        if (client == null)
        {
            throw new TinyidpTokenException("Client id unknown", "invalid_client");
        }

        ITokenStrategy? tokenStrategy = null;
        switch(request.grant_type)
        {
            case "client_credential":
                tokenStrategy = GetTokenStrategy(TokenTypeEnum.client_credential);
                break;
            case "code":
            case "authorization_code":
                tokenStrategy = GetTokenStrategy(TokenTypeEnum.code);
                break;
            case "pkce":
                tokenStrategy = GetTokenStrategy(TokenTypeEnum.client_credential);
                break;
            case "refresh_token":
                tokenStrategy = GetTokenStrategy(TokenTypeEnum.refresh_token);
                break;
        }
        if (tokenStrategy == null)
            throw new TinyidpTokenException("grant_type is not implemented", "unsupported_grant_type");
        TokenResponseBusiness resp = await tokenStrategy.GetTokenByType(httpContext, request, client);

        resp.refresh_token = GenerateRefreshToken(resp.refreshTokenResponse);

        client.RefreshToken = resp.refresh_token;
        client.CreationDateRefreshToken = DateTime.Now;
        _credentialBusiness.Update(client);
        
        return resp;
    }

    public ITokenStrategy GetTokenStrategy(TokenTypeEnum tokenType)
    {
        return _serviceProvider.GetRequiredKeyedService<ITokenStrategy>(tokenType);
    }

    public string GenerateRefreshToken(RefreshTokenResponse rtoken)
    {
        string serializedRtoken = JsonSerializer.Serialize(rtoken);
        return _encryptionService.Encrypt(serializedRtoken);
    }    
}