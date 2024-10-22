using System;
using System;
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

public class TokenAuthorizationCodeTest
{
    private readonly Mock<IConfiguration> _confMock;
    private readonly Mock<ILogger<TokenAuthorizationCode>> _loggerMock;
    private readonly Mock<IKeysManagment> _keysManagmentMock;
    private readonly Mock<ITokenRepository> _tokenRepositoryMock;
    private readonly Mock<ICredentialBusiness> _credentialBusinessMock;
    private readonly Mock<IPKCEService> _pkceServiceMock;
    private readonly TokenAuthorizationCode _tokenAuthorizationCode;

    public TokenAuthorizationCodeTest()
    {
        _loggerMock = new Mock<ILogger<TokenAuthorizationCode>>();
        _confMock = new Mock<IConfiguration>();
        _keysManagmentMock = new Mock<IKeysManagment>();
        _tokenRepositoryMock = new Mock<ITokenRepository>();
        _credentialBusinessMock = new Mock<ICredentialBusiness>();
        _pkceServiceMock = new Mock<IPKCEService>();

        _tokenAuthorizationCode = new TokenAuthorizationCode(
            _confMock.Object, 
            _loggerMock.Object, 
            _keysManagmentMock.Object, 
            _tokenRepositoryMock.Object, 
            _credentialBusinessMock.Object,
            _pkceServiceMock.Object);
    }

    [Fact]
    public void GetTokenByType_NoScope_ReturnException()
    {
        TokenRequestBusiness request = new TokenRequestBusiness{
             client_id = "Test9", client_secret = "Test9", grant_type = "token", redirect_uri = "https://localhost", scope = new List<string> {""}
        };
        CredentialBusinessEntity client = new CredentialBusinessEntity {
             Id = 1, Ident = "Test9", State = StateCredential.Active, KeyType = AlgoKeyType.ES256,
             AllowedScopes = new List<string> { "scope1"},
             Audiences = new List<string> { "aud1"},
             TokenMaxMinuteValidity = 3600
        };

        Assert.Throws<TinyidpTokenException>( () =>  _tokenAuthorizationCode.GetTokenByType(request, client));
    }

    [Fact]
    public void GetTokenByType_CodeVerifierEmpty_ReturnException()
    {
        TokenRequestBusiness request = new TokenRequestBusiness{
             client_id = "Test9", client_secret = "Test9", grant_type = "token", redirect_uri = "https://localhost", scope = new List<string> {"scope1"}, code_verifier = ""
        };
        CredentialBusinessEntity client = new CredentialBusinessEntity {
             Id = 1, Ident = "Test9", State = StateCredential.Active, KeyType = AlgoKeyType.ES256,
             AllowedScopes = new List<string> { "scope1"},
             Audiences = new List<string> { "aud1"},
             TokenMaxMinuteValidity = 3600,
             CodeChallenge = "codeverifier"
        };

        TinyidpTokenException ex = Assert.Throws<TinyidpTokenException>( () =>  _tokenAuthorizationCode.GetTokenByType(request, client));
        Assert.Equal("Client need code verifier", ex.Message);
        Assert.Equal("invalid_request", ex.error_description);
    }

    [Fact]
    public void GetTokenByType_S256_InvalidPKCE_ReturnException()
    {
        TokenRequestBusiness request = new TokenRequestBusiness{
             client_id = "Test9", client_secret = "Test9", grant_type = "token", redirect_uri = "https://localhost", scope = new List<string> {"scope1"}, code_verifier = "Zxo4fgAXWrCPd2Y4CKhXS4B2zLgiRkBPKUmff4ah5to"
        };
        CredentialBusinessEntity client = new CredentialBusinessEntity {
             Id = 1, Ident = "Test9", State = StateCredential.Active, KeyType = AlgoKeyType.ES256,
             AllowedScopes = new List<string> { "scope1"},
             Audiences = new List<string> { "aud1"},
             TokenMaxMinuteValidity = 3600,
             CodeChallengeMethod = "S256",
             CodeChallenge = "code-verifier"
        };
        _pkceServiceMock.Setup(x => x.ValidPKCE(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        TinyidpTokenException ex = Assert.Throws<TinyidpTokenException>( () =>  _tokenAuthorizationCode.GetTokenByType(request, client));

        Assert.Equal("Challenge PKCE not corresponding", ex.Message);
    }

    [Fact]
    public void GetTokenByType_Plain_InvalidPKCE_ReturnException()
    {
        TokenRequestBusiness request = new TokenRequestBusiness{
             client_id = "Test9", client_secret = "Test9", grant_type = "token", redirect_uri = "https://localhost", scope = new List<string> {"scope1"}, code_verifier = "Zxo4fgAXWrCPd2Y4CKhXS4B2zLgiRkBPKUmff4ah5to"
        };
        CredentialBusinessEntity client = new CredentialBusinessEntity {
             Id = 1, Ident = "Test9", State = StateCredential.Active, KeyType = AlgoKeyType.ES256,
             AllowedScopes = new List<string> { "scope1"},
             Audiences = new List<string> { "aud1"},
             TokenMaxMinuteValidity = 3600,
             CodeChallengeMethod = "plain",
             CodeChallenge = "code-verifier"
        };

        TinyidpTokenException ex = Assert.Throws<TinyidpTokenException>( () =>  _tokenAuthorizationCode.GetTokenByType(request, client));

        Assert.Equal("Challenge PKCE not corresponding", ex.Message);
    }

    [Fact]
    public void GetTokenByType_NoAuthorizationCode_ReturnException()
    {
        TokenRequestBusiness request = new TokenRequestBusiness{
             client_id = "Test9", client_secret = "Test9", grant_type = "token", redirect_uri = "https://localhost", scope = new List<string> {"scope1"}, code_verifier = "Zxo4fgAXWrCPd2Y4CKhXS4B2zLgiRkBPKUmff4ah5to"
        };
        CredentialBusinessEntity client = new CredentialBusinessEntity {
             Id = 1, Ident = "Test9", State = StateCredential.Active, KeyType = AlgoKeyType.ES256,
             AllowedScopes = new List<string> { "scope1"},
             Audiences = new List<string> { "aud1"},
             TokenMaxMinuteValidity = 3600,
             CodeChallengeMethod = "S256",
             CodeChallenge = "code-verifier"
        };
        _pkceServiceMock.Setup(x => x.ValidPKCE(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        TinyidpTokenException ex = Assert.Throws<TinyidpTokenException>( () =>  _tokenAuthorizationCode.GetTokenByType(request, client));

        Assert.Equal("No authorization code", ex.Message);
    }

    [Fact]
    public void GetTokenByType_NoUser_ReturnException()
    {
        TokenRequestBusiness request = new TokenRequestBusiness{
             client_id = "Test9", 
             client_secret = "Test9", 
             grant_type = "token", 
             redirect_uri = "https://localhost", 
             scope = new List<string> {"scope1"}, 
             code_verifier = "Zxo4fgAXWrCPd2Y4CKhXS4B2zLgiRkBPKUmff4ah5to",
             code = "HIUHGKJK"
        };
        CredentialBusinessEntity client = new CredentialBusinessEntity {
             Id = 1, Ident = "Test9", State = StateCredential.Active, KeyType = AlgoKeyType.ES256,
             AllowedScopes = new List<string> { "scope1"},
             Audiences = new List<string> { "aud1"},
             TokenMaxMinuteValidity = 3600,
             CodeChallengeMethod = "S256",
             CodeChallenge = "code-verifier"
        };
        CredentialBusinessEntity? user = null!;
        _pkceServiceMock.Setup(x => x.ValidPKCE(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _credentialBusinessMock.Setup(x => x.GetByAuthorizationCode(It.IsAny<string>())).Returns(Task.FromResult<CredentialBusinessEntity?>(user));

        TinyidpTokenException ex = Assert.Throws<TinyidpTokenException>( () =>  _tokenAuthorizationCode.GetTokenByType(request, client));

        Assert.Equal("Authorization code not found", ex.Message);
    }

    [Fact]
    public void GetTokenByType_Scope_ReturnOk()
    {
        TokenRequestBusiness request = new TokenRequestBusiness{
             client_id = "Test9", 
             client_secret = "Test9", 
             grant_type = "token", 
             redirect_uri = "https://localhost", 
             scope = new List<string> {"scope1"}, 
             code_verifier = "Zxo4fgAXWrCPd2Y4CKhXS4B2zLgiRkBPKUmff4ah5to",
             code = "HIUHGKJK"
        };
        _keysManagmentMock.Setup(x => x.GenerateJWTToken(
            It.IsAny<AlgoKeyType>(),
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<string?>(),
            It.IsAny<long>(),
            null
        )).Returns("JKHKJHKJH.HGYFFUYFUFU.UGUGJJGHJG");
        
        CredentialBusinessEntity client = new CredentialBusinessEntity {
             Id = 1, Ident = "Test9", State = StateCredential.Active, KeyType = AlgoKeyType.ES256,
             AllowedScopes = new List<string> { "scope1"},
             Audiences = new List<string> { "aud1"},
             TokenMaxMinuteValidity = 3600
        };
        CredentialBusinessEntity user = new CredentialBusinessEntity {
             Id = 1, Ident = "Test1", State = StateCredential.Active, KeyType = AlgoKeyType.ES256
        };
        _pkceServiceMock.Setup(x => x.ValidPKCE(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _credentialBusinessMock.Setup(x => x.GetByAuthorizationCode(It.IsAny<string>())).Returns(Task.FromResult<CredentialBusinessEntity?>(user));

        TokenResponseBusiness resp = _tokenAuthorizationCode.GetTokenByType(request, client);

        Assert.NotEmpty(resp.access_token);
        Assert.NotNull(resp.access_token);
    }

    [Fact]
    public void VerifyClientIdent_RoleDifferent_ReturnException()
    {
        BasicIdent ident = new BasicIdent {
            ClientId = "Test9", ClientSecret = "Test9"
        };
        TokenRequestBusiness request = new TokenRequestBusiness {
            client_id = "Test9", client_secret = "Test9"
        };
        CredentialBusinessEntity client =  new CredentialBusinessEntity {
             Id = 1, Ident = "Test9", RoleIdent = RoleCredential.User
        };
        bool checkedPwd = false;    

        TinyidpTokenException ex = Assert.Throws<TinyidpTokenException>( 
            () => _tokenAuthorizationCode.VerifyClientIdent(ident, request, client, checkedPwd));

        Assert.Equal("Only client role can use client_credential", ex.Message);
        Assert.Equal("unsupported_grant_type", ex.error_description);
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
            () => _tokenAuthorizationCode.VerifyClientIdent(ident, request, client, checkedPwd));

        Assert.Equal("Client_id of the request is not the same than Authorization header", ex.Message);
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

        bool res = _tokenAuthorizationCode.VerifyClientIdent(ident, request, client, checkedPwd);

        Assert.True(res);
    }

    [Fact]
    public void VerifyClientIdent_BadFormatSecret_ReturnException()
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
        bool checkedPwd = true;    

        TinyidpTokenException ex = Assert.Throws<TinyidpTokenException>( 
            () => _tokenAuthorizationCode.VerifyClientIdent(ident, request, client, checkedPwd));

        Assert.Equal("Bad secret format", ex.Message);
        Assert.Equal("invalid_request", ex.error_description);
    }

    [Fact]
    public void VerifyClientIdent_ClientSecretDifferent_ReturnException()
    {
        BasicIdent ident = new BasicIdent {
            ClientId = "Test9", ClientSecret = "Test9"
        };
        TokenRequestBusiness request = new TokenRequestBusiness {
            client_id = "Test9", client_secret = "VGVzdDg="
        };
        CredentialBusinessEntity client =  new CredentialBusinessEntity {
             Id = 1, Ident = "Test9", RoleIdent = RoleCredential.Client
        };
        bool checkedPwd = true;    

        TinyidpTokenException ex = Assert.Throws<TinyidpTokenException>( 
            () => _tokenAuthorizationCode.VerifyClientIdent(ident, request, client, checkedPwd));

        Assert.Equal("client_secret of the request is not the same than Authorization header", ex.Message);
        Assert.Equal("invalid_request", ex.error_description);
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

        bool res = _tokenAuthorizationCode.VerifyClientIdent(ident, request, client, checkedPwd);

        Assert.True(res);
    }
}
