using System;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using tinyidp.Business.BusinessEntities;
using tinyidp.Business.Credential;
using tinyidp.Business.tokens;
using tinyidp.Encryption;
using tinyidp.Exceptions;
using tinyidp.infrastructure.bdd;
using tinyidp.infrastructure.keysmanagment;

namespace tinyidp.tests;

public class RefreshTokenTest
{
    private readonly Mock<IConfiguration> _confMock;
    private readonly Mock<ILogger<RefreshToken>> _loggerMock;
    private readonly Mock<IKeysManagment> _keysManagmentMock;
    private readonly Mock<ITokenRepository> _tokenRepositoryMock;
    private readonly Mock<ICredentialBusiness> _credentialBusinessMock;
    private readonly Mock<IEncryptionService> _encryptionServiceMock;
    private readonly RefreshToken _refreshToken;

    public RefreshTokenTest()
    {
        _loggerMock = new Mock<ILogger<RefreshToken>>();
        _confMock = new Mock<IConfiguration>();
        _keysManagmentMock = new Mock<IKeysManagment>();
        _tokenRepositoryMock = new Mock<ITokenRepository>();
        _credentialBusinessMock = new Mock<ICredentialBusiness>();
        _encryptionServiceMock = new Mock<IEncryptionService>(); 

        _refreshToken = new RefreshToken(
            _confMock.Object, 
            _loggerMock.Object, 
            _keysManagmentMock.Object, 
            _tokenRepositoryMock.Object, 
            _credentialBusinessMock.Object,
            _encryptionServiceMock.Object);
    }

    [Fact]
    public void GetTokenByType_CreationDateInconsistent_ReturnException()
    {
        TokenRequestBusiness request = new TokenRequestBusiness{
             client_id = "Test9", client_secret = "Test9", grant_type = "token", redirect_uri = "https://localhost", scope = new List<string> {""}
        };
        CredentialBusinessEntity client = new CredentialBusinessEntity {
             Id = 1, Ident = "Test9", State = StateCredential.Active, KeyType = AlgoKeyType.ES256,
             AllowedScopes = new List<string> { "scope1"},
             Audiences = new List<string> { "aud1"},
             TokenMaxMinuteValidity = 3600,
             CreationDateRefreshToken = null
        };

        TinyidpTokenException ex = Assert.Throws<TinyidpTokenException>( () =>  _refreshToken.GetTokenByType(request, client));

        Assert.Equal("Token and creation date are inconsistent", ex.Message);
        Assert.Equal("invalid_token", ex.error_description);
    }

    [Fact]
    public void GetTokenByType_RefreshTokenExpire_ReturnException()
    {
        TokenRequestBusiness request = new TokenRequestBusiness{
             client_id = "Test9", client_secret = "Test9", grant_type = "token", redirect_uri = "https://localhost", scope = new List<string> {""}
        };
        CredentialBusinessEntity client = new CredentialBusinessEntity {
             Id = 1, Ident = "Test9", State = StateCredential.Active, KeyType = AlgoKeyType.ES256,
             AllowedScopes = new List<string> { "scope1"},
             Audiences = new List<string> { "aud1"},
             TokenMaxMinuteValidity = 3600,
             CreationDateRefreshToken = DateTime.Now.AddDays(-1),
             RefreshMaxMinuteValidity = 60
        };

        TinyidpTokenException ex = Assert.Throws<TinyidpTokenException>( () =>  _refreshToken.GetTokenByType(request, client));

        Assert.Equal("Refresh token expired", ex.Message);
        Assert.Equal("invalid_token", ex.error_description);
    }

    [Fact]
    public void GetTokenByType_WrongRefreshToken_ReturnException()
    {
        TokenRequestBusiness request = new TokenRequestBusiness{
             client_id = "Test9", client_secret = "Test9", grant_type = "token", redirect_uri = "https://localhost", scope = new List<string> {""}, refresh_token = "kjlkjlkjhjggjhg"
        };
        CredentialBusinessEntity client = new CredentialBusinessEntity {
             Id = 1, Ident = "Test9", State = StateCredential.Active, KeyType = AlgoKeyType.ES256,
             AllowedScopes = new List<string> { "scope1"},
             Audiences = new List<string> { "aud1"},
             TokenMaxMinuteValidity = 3600,
             CreationDateRefreshToken = DateTime.Now.AddMinutes(-5),
             RefreshMaxMinuteValidity = 60
        };
        string refreshTokenDecrypted = null!;
        _encryptionServiceMock.Setup(x => x.Decrypt(It.IsAny<string>())).Returns(refreshTokenDecrypted);

        TinyidpTokenException ex = Assert.Throws<TinyidpTokenException>( () =>  _refreshToken.GetTokenByType(request, client));

        Assert.Equal("Error when decoding refresh_token", ex.Message);
        Assert.Equal("invalid_token", ex.error_description);
    }

    [Fact]
    public void GetTokenByType_Scope_ReturnOk()
    {
        TokenRequestBusiness request = new TokenRequestBusiness{
             client_id = "Test9", client_secret = "Test9", grant_type = "token", redirect_uri = "https://localhost", scope = new List<string> {"scope1"}
        };
        CredentialBusinessEntity client = new CredentialBusinessEntity {
             Id = 1, Ident = "Test9", State = StateCredential.Active, KeyType = AlgoKeyType.ES256,
             AllowedScopes = new List<string> { "scope1"},
             Audiences = new List<string> { "aud1"},
             TokenMaxMinuteValidity = 3600,
             CreationDateRefreshToken = DateTime.Now.AddMinutes(-5),
             RefreshMaxMinuteValidity = 60
        };
        RefreshTokenResponse respToken = new RefreshTokenResponse {
             Algo = AlgoKeyType.ES256, Audiences = new List<string> { "aud1" }, Ident = "Test9", Scopes = new List<string> { "scope1" }, LifeTime = 3600
        };
        string refreshTokenDecrypted = JsonSerializer.Serialize(respToken);
        _encryptionServiceMock.Setup(x => x.Decrypt(It.IsAny<string>())).Returns(refreshTokenDecrypted);
        _keysManagmentMock.Setup(x => x.GenerateJWTToken(
            It.IsAny<AlgoKeyType>(),
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<string?>(),
            It.IsAny<long>(),
            null
        )).Returns("JKHKJHKJH.HGYFFUYFUFU.UGUGJJGHJG");
        
        TokenResponseBusiness resp = _refreshToken.GetTokenByType(request, client);

        Assert.NotEmpty(resp.access_token);
        Assert.NotNull(resp.access_token);
    }

    [Fact]
    public void VerifyClientIdent_ClientIdDifferent_ReturnException()
    {
        BasicIdent ident = new BasicIdent {
            ClientId = "Test1", ClientSecret = "Test9"
        };
        TokenRequestBusiness request = new TokenRequestBusiness {
            client_id = "Test9", client_secret = "Test9"
        };
        CredentialBusinessEntity client =  new CredentialBusinessEntity {
             Id = 1, Ident = "Test9", RoleIdent = RoleCredential.Client
        };
        bool checkedPwd = false;    

        TinyidpTokenException ex = Assert.Throws<TinyidpTokenException>( 
            () => _refreshToken.VerifyClientIdent(ident, request, client, checkedPwd));

        Assert.Equal("Client corresponding of the refresh_token is not the same than Authorization header", ex.Message);
        Assert.Equal("invalid_request", ex.error_description);
    }

    [Fact]
    public void VerifyClientIdent_ChkPwdFalse_ReturnOk()
    {
        BasicIdent ident = new BasicIdent {
            ClientId = "Test9", ClientSecret = "Test9"
        };
        TokenRequestBusiness request = new TokenRequestBusiness {
            client_id = "Test9", client_secret = "Test9"
        };
        CredentialBusinessEntity client =  new CredentialBusinessEntity {
             Id = 1, Ident = "Test9", RoleIdent = RoleCredential.Client
        };
        bool checkedPwd = false;    

        bool res = _refreshToken.VerifyClientIdent(ident, request, client, checkedPwd);

        Assert.True(res);
    }

    [Fact]
    public void VerifyClientIdent_ReturnOk()
    {
        BasicIdent ident = new BasicIdent {
            ClientId = "Test9", ClientSecret = "Test9"
        };
        TokenRequestBusiness request = new TokenRequestBusiness {
            client_id = "Test9", client_secret = "VGVzdDk="
        };
        CredentialBusinessEntity client =  new CredentialBusinessEntity {
             Id = 1, Ident = "Test9", RoleIdent = RoleCredential.Client
        };
        bool checkedPwd = true;
        _credentialBusinessMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);    

        bool res = _refreshToken.VerifyClientIdent(ident, request, client, checkedPwd);

        Assert.True(res);
    }
}