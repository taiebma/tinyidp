using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using tinyidp.Business.BusinessEntities;
using tinyidp.Business.Credential;
using tinyidp.Business.tokens;
using tinyidp.Exceptions;
using tinyidp.infrastructure.bdd;
using tinyidp.infrastructure.keysmanagment;

namespace tinyidp.tests;

public class TokenClientCredentialTest
{
    private readonly Mock<IConfiguration> _confMock;
    private readonly Mock<ILogger<TokenClientCredential>> _loggerMock;
    private readonly Mock<IKeysManagment> _keysManagmentMock;
    private readonly Mock<ITokenRepository> _tokenRepositoryMock;
    private readonly Mock<ICredentialBusiness> _credentialBusinessMock;
    private readonly TokenClientCredential _tokenClientCredential;

    public TokenClientCredentialTest()
    {
        _loggerMock = new Mock<ILogger<TokenClientCredential>>();
        _confMock = new Mock<IConfiguration>();
        _keysManagmentMock = new Mock<IKeysManagment>();
        _tokenRepositoryMock = new Mock<ITokenRepository>();
        _credentialBusinessMock = new Mock<ICredentialBusiness>();

        _tokenClientCredential = new TokenClientCredential(
            _confMock.Object, 
            _loggerMock.Object, 
            _keysManagmentMock.Object, 
            _tokenRepositoryMock.Object, 
            _credentialBusinessMock.Object);
    }

    [Fact]
    public void GetTokenByType_NoScope_ReturnException()
    {
        TokenRequestBusiness request = new TokenRequestBusiness{
             client_id = "Test9", client_secret = "Test9", grant_type = "token", redirect_uri = "https://localhost", scope = new List<string> {""}
        };
        tinyidp.infrastructure.bdd.Credential client = new tinyidp.infrastructure.bdd.Credential {
             Id = 1, Ident = "Test9", State = (int)StateCredential.Active, KeyType = (int)AlgoKeyType.ES256,
             AllowedScopes = "scope1",
             Audiences = "aud1",
             TokenMaxMinuteValidity = 3600
        };

        Assert.Throws<TinyidpTokenException>( () =>  _tokenClientCredential.GetTokenByType(request, client));
    }

    [Fact]
    public void GetTokenByType_Scope_ReturnOk()
    {
        TokenRequestBusiness request = new TokenRequestBusiness{
             client_id = "Test9", client_secret = "Test9", grant_type = "token", redirect_uri = "https://localhost", scope = new List<string> {"scope1"}
        };
        _keysManagmentMock.Setup(x => x.GenerateJWTToken(
            It.IsAny<AlgoKeyType>(),
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<string?>(),
            It.IsAny<long>(),
            null
        )).Returns("JKHKJHKJH.HGYFFUYFUFU.UGUGJJGHJG");

        tinyidp.infrastructure.bdd.Credential client = new tinyidp.infrastructure.bdd.Credential {
             Id = 1, Ident = "Test9", State = (int)StateCredential.Active, KeyType = (int)AlgoKeyType.ES256,
             AllowedScopes = "scope1",
             Audiences = "aud1",
             TokenMaxMinuteValidity = 3600
        };

        TokenResponseBusiness resp = _tokenClientCredential.GetTokenByType(request, client);

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
        tinyidp.infrastructure.bdd.Credential client =  new tinyidp.infrastructure.bdd.Credential {
             Id = 1, Ident = "Test9", RoleIdent = (int)RoleCredential.User
        };
        bool checkedPwd = false;    

        TinyidpTokenException ex = Assert.Throws<TinyidpTokenException>( 
            () => _tokenClientCredential.VerifyClientIdent(ident, request, client, checkedPwd));

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
        tinyidp.infrastructure.bdd.Credential client =  new tinyidp.infrastructure.bdd.Credential {
             Id = 1, Ident = "Test9", RoleIdent = (int)RoleCredential.Client
        };
        bool checkedPwd = false;    

        TinyidpTokenException ex = Assert.Throws<TinyidpTokenException>( 
            () => _tokenClientCredential.VerifyClientIdent(ident, request, client, checkedPwd));

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
        tinyidp.infrastructure.bdd.Credential client =  new tinyidp.infrastructure.bdd.Credential {
             Id = 1, Ident = "Test9", RoleIdent = (int)RoleCredential.Client
        };
        bool checkedPwd = false;    

        bool res = _tokenClientCredential.VerifyClientIdent(ident, request, client, checkedPwd);

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
        tinyidp.infrastructure.bdd.Credential client =  new tinyidp.infrastructure.bdd.Credential {
             Id = 1, Ident = "Test9", RoleIdent = (int)RoleCredential.Client
        };
        bool checkedPwd = true;    

        TinyidpTokenException ex = Assert.Throws<TinyidpTokenException>( 
            () => _tokenClientCredential.VerifyClientIdent(ident, request, client, checkedPwd));

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
        tinyidp.infrastructure.bdd.Credential client =  new tinyidp.infrastructure.bdd.Credential {
             Id = 1, Ident = "Test9", RoleIdent = (int)RoleCredential.Client
        };
        bool checkedPwd = true;    

        TinyidpTokenException ex = Assert.Throws<TinyidpTokenException>( 
            () => _tokenClientCredential.VerifyClientIdent(ident, request, client, checkedPwd));

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
        tinyidp.infrastructure.bdd.Credential client =  new tinyidp.infrastructure.bdd.Credential {
             Id = 1, Ident = "Test9", RoleIdent = (int)RoleCredential.Client
        };
        bool checkedPwd = true;
        _credentialBusinessMock.Setup(x => x.CheckPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);    

        bool res = _tokenClientCredential.VerifyClientIdent(ident, request, client, checkedPwd);

        Assert.True(res);
    }
}
