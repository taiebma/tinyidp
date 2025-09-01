using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

public class TokenServiceTest
{
    private readonly Mock<IConfiguration> _confMock;
    private readonly Mock<ILogger<TokenService>> _loggerMock;
    private readonly Mock<IKeyedServiceProvider> _serviceProviderMock;
    private readonly Mock<IEncryptionService> _encryptionServiceMock;
    private readonly Mock<ITokenRepository> _tokenRepositoryMock;
    private readonly Mock<ICredentialBusiness> _credentialBusinessMock;
    private readonly Mock<IThrustStoreService> _thrustStoreServiceMock;
    private readonly Mock<ITokenStrategy> _tokenStrategyMock;
    private Mock<HttpContext> _contextMock;
    private Mock<HttpRequest> _requestMock;
    private Mock<IHeaderDictionary> _headerMock;
    private Mock<ConnectionInfo> _connectionInfoMock;

    private readonly ITokenService _tokenService;

    public TokenServiceTest()
    {
        _confMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<TokenService>>();
        _serviceProviderMock = new Mock<IKeyedServiceProvider>();
        _encryptionServiceMock = new Mock<IEncryptionService>();
        _tokenRepositoryMock = new Mock<ITokenRepository>();
        _credentialBusinessMock = new Mock<ICredentialBusiness>();
        _thrustStoreServiceMock = new Mock<IThrustStoreService>();
        _tokenStrategyMock = new Mock<ITokenStrategy>();
        _contextMock = new Mock<HttpContext>();
        _requestMock = new Mock<HttpRequest>();
        _headerMock = new Mock<IHeaderDictionary>();
        _connectionInfoMock = new Mock<ConnectionInfo>();

        _tokenService  = new TokenService(
            _confMock.Object,
            _loggerMock.Object,
            _serviceProviderMock.Object,
            _encryptionServiceMock.Object,
            _tokenRepositoryMock.Object,
            _credentialBusinessMock.Object,
            _thrustStoreServiceMock.Object
        );
    }

    [Fact]
    public async Task GetToken_NoHttpContext_ReturnException()
    {
        TokenRequestBusiness request = new TokenRequestBusiness {
             client_id = "Test9",
             grant_type = "code"
        };
        _headerMock.Setup(x => x["Authorization"]).Returns("Basic VGVzdDE6VGVzdDFUZXN0MSE=");
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);

        TinyidpTokenException ex = await Assert.ThrowsAsync<TinyidpTokenException>( async () => await _tokenService.GetToken(null, request));

        Assert.Equal("No HTTP Context", ex.Message);

    }

    [Fact]
    public async Task GetToken_NoAuthorizationHeader_ReturnException()
    {
        TokenRequestBusiness request = new TokenRequestBusiness {
             client_id = "Test9",
             grant_type = "code"
        };
        X509Certificate2 cert = null!;
        string headerAuth = null!;
        _headerMock.Setup(x => x["Authorization"]).Returns(headerAuth);
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _connectionInfoMock.Setup(x => x.ClientCertificate).Returns(cert);
        _contextMock.Setup(x => x.Connection).Returns(_connectionInfoMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);

        TinyidpTokenException ex = await Assert.ThrowsAsync<TinyidpTokenException>( async () => await _tokenService.GetToken(_contextMock.Object, request));

        Assert.Equal("No Authorization header", ex.Message);
        Assert.Equal("invalid_request", ex.error_description);

    }

    [Fact]
    public async Task GetToken_MustBasic_ReturnException()
    {
        TokenRequestBusiness request = new TokenRequestBusiness {
             client_id = "Test9",
             grant_type = "code"
        };
        X509Certificate2 cert = null!;
        _headerMock.Setup(x => x["Authorization"]).Returns("VGVzdDE6VGVzdDFUZXN0MSE=");
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _connectionInfoMock.Setup(x => x.ClientCertificate).Returns(cert);
        _contextMock.Setup(x => x.Connection).Returns(_connectionInfoMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);

        TinyidpTokenException ex = await Assert.ThrowsAsync<TinyidpTokenException>( async () => await _tokenService.GetToken(_contextMock.Object, request));

        Assert.Equal("For client_credential grant_type, Authorization must be Basic", ex.Message);
        Assert.Equal("invalid_request", ex.error_description);

    }

    [Fact]
    public async Task GetToken_BadFormatAuth_ReturnException()
    {
        TokenRequestBusiness request = new TokenRequestBusiness {
             client_id = "Test9",
             grant_type = "code"
        };
        X509Certificate2 cert = null!;
        _headerMock.Setup(x => x["Authorization"]).Returns("Basic VGVzdDFUZXN0MVRlc3QxIQ==");
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _connectionInfoMock.Setup(x => x.ClientCertificate).Returns(cert);
        _contextMock.Setup(x => x.Connection).Returns(_connectionInfoMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);

        TinyidpTokenException ex = await Assert.ThrowsAsync<TinyidpTokenException>( async () => await _tokenService.GetToken(_contextMock.Object, request));

        Assert.Equal("Basic Authorization must be <client_id>:<client_secret> format", ex.Message);
        Assert.Equal("invalid_request", ex.error_description);

    }

    [Fact]
    public async Task GetToken_NoStrategy_ReturnException()
    {
        TokenRequestBusiness request = new TokenRequestBusiness {
             client_id = "Test9",
             grant_type = ""
        };
        tinyidp.infrastructure.bdd.Credential? client = new tinyidp.infrastructure.bdd.Credential{
             Ident = "Test9"
        };
        TokenResponseBusiness tokenResp = new TokenResponseBusiness {
             access_token = "HKJHKJHKJHKJHK",
             refresh_token = "KLJLJLKJLKJL"
        };
        X509Certificate2 cert = null!;
        _headerMock.Setup(x => x["Authorization"]).Returns("Basic VGVzdDE6VGVzdDFUZXN0MSE=");
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _connectionInfoMock.Setup(x => x.ClientCertificate).Returns(cert);
        _contextMock.Setup(x => x.Connection).Returns(_connectionInfoMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);
        _tokenStrategyMock.Setup(x => x.GetTokenByType(It.IsAny<TokenRequestBusiness>(), It.IsAny<tinyidp.infrastructure.bdd.Credential>())).Returns(tokenResp);
        _tokenStrategyMock.Setup(x => x.VerifyClientIdent(It.IsAny<BasicIdent>(), It.IsAny<TokenRequestBusiness>(), It.IsAny<tinyidp.infrastructure.bdd.Credential>(), It.IsAny<bool>())).Returns(true);
        _serviceProviderMock.Setup(x => x.GetService(typeof(TokenAuthorizationCode))).Returns(_tokenStrategyMock.Object);

        _credentialBusinessMock.Setup(x => x.GetByIdent(It.IsAny<string>())).Returns(Task.FromResult<tinyidp.infrastructure.bdd.Credential?>(client));

        TinyidpTokenException ex = await Assert.ThrowsAsync<TinyidpTokenException>( async () => await _tokenService.GetToken(_contextMock.Object, request));

        Assert.Equal("grant_type is not implemented", ex.Message);
        Assert.Equal("unsupported_grant_type", ex.error_description);

    }

    [Fact]
    public async Task GetToken_NoClient_ReturnException()
    {
        TokenRequestBusiness request = new TokenRequestBusiness {
             client_id = "Test9",
             grant_type = "code"
        };
        tinyidp.infrastructure.bdd.Credential? client = null;
        TokenResponseBusiness tokenResp = new TokenResponseBusiness {
             access_token = "HKJHKJHKJHKJHK",
             refresh_token = "KLJLJLKJLKJL"
        };
        X509Certificate2 cert = null!;
        _headerMock.Setup(x => x["Authorization"]).Returns("Basic VGVzdDE6VGVzdDFUZXN0MSE=");
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _connectionInfoMock.Setup(x => x.ClientCertificate).Returns(cert);
        _contextMock.Setup(x => x.Connection).Returns(_connectionInfoMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);
        _tokenStrategyMock.Setup(x => x.GetTokenByType(It.IsAny<TokenRequestBusiness>(), It.IsAny<tinyidp.infrastructure.bdd.Credential>())).Returns(tokenResp);
        _tokenStrategyMock.Setup(x => x.VerifyClientIdent(It.IsAny<BasicIdent>(), It.IsAny<TokenRequestBusiness>(), It.IsAny<tinyidp.infrastructure.bdd.Credential>(), It.IsAny<bool>())).Returns(true);
        _serviceProviderMock.Setup(x => x.GetRequiredKeyedService(It.IsAny<Type>(), It.IsAny<object?>())).Returns(_tokenStrategyMock.Object);

        _credentialBusinessMock.Setup(x => x.GetByIdent(It.IsAny<string>())).Returns(Task.FromResult<tinyidp.infrastructure.bdd.Credential?>(client));

        TinyidpTokenException ex = await Assert.ThrowsAsync<TinyidpTokenException>( async () => await _tokenService.GetToken(_contextMock.Object, request));

        Assert.Equal("Client id unknown", ex.Message);
        Assert.Equal("invalid_client", ex.error_description);

    }

    [Fact]
    public async Task GetToken_VerifyClientKo_ReturnException()
    {
        TokenRequestBusiness request = new TokenRequestBusiness {
             client_id = "Test9",
             grant_type = "code"
        };
        tinyidp.infrastructure.bdd.Credential? client = new tinyidp.infrastructure.bdd.Credential{
             Ident = "Test9"
        };
        TokenResponseBusiness tokenResp = new TokenResponseBusiness {
             access_token = "HKJHKJHKJHKJHK",
             refresh_token = "KLJLJLKJLKJL"
        };
        X509Certificate2 cert = null!;
        _headerMock.Setup(x => x["Authorization"]).Returns("Basic VGVzdDE6VGVzdDFUZXN0MSE=");
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _connectionInfoMock.Setup(x => x.ClientCertificate).Returns(cert);
        _contextMock.Setup(x => x.Connection).Returns(_connectionInfoMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);
        _tokenStrategyMock.Setup(x => x.GetTokenByType(It.IsAny<TokenRequestBusiness>(), It.IsAny<tinyidp.infrastructure.bdd.Credential>())).Returns(tokenResp);
        _tokenStrategyMock.Setup(x => x.VerifyClientIdent(It.IsAny<BasicIdent>(), It.IsAny<TokenRequestBusiness>(), It.IsAny<tinyidp.infrastructure.bdd.Credential>(), It.IsAny<bool>())).Returns(false);
        _serviceProviderMock.Setup(x => x.GetRequiredKeyedService(It.IsAny<Type>(), It.IsAny<object?>())).Returns(_tokenStrategyMock.Object);

        _credentialBusinessMock.Setup(x => x.GetByIdent(It.IsAny<string>())).Returns(Task.FromResult<tinyidp.infrastructure.bdd.Credential?>(client));

        TinyidpTokenException ex = await Assert.ThrowsAsync<TinyidpTokenException>( async () => await _tokenService.GetToken(_contextMock.Object, request));

        Assert.Equal("Client unauthorized", ex.Message);
        Assert.Equal("unauthorized_client", ex.error_description);

    }

    [Fact]
    public async Task GetToken_ReturnOk()
    {
        TokenRequestBusiness request = new TokenRequestBusiness {
             client_id = "Test9",
             grant_type = "code"
        };
        tinyidp.infrastructure.bdd.Credential? client = new tinyidp.infrastructure.bdd.Credential{
             Ident = "Test9"
        };
        TokenResponseBusiness tokenResp = new TokenResponseBusiness {
             access_token = "HKJHKJHKJHKJHK",
             refresh_token = "KLJLJLKJLKJL"
        };
        X509Certificate2 cert = null!;
        _headerMock.Setup(x => x["Authorization"]).Returns("Basic VGVzdDE6VGVzdDFUZXN0MSE=");
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _connectionInfoMock.Setup(x => x.ClientCertificate).Returns(cert);
        _contextMock.Setup(x => x.Connection).Returns(_connectionInfoMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);
        _tokenStrategyMock.Setup(x => x.GetTokenByType(It.IsAny<TokenRequestBusiness>(), It.IsAny<tinyidp.infrastructure.bdd.Credential>())).Returns(tokenResp);
        _tokenStrategyMock.Setup(x => x.VerifyClientIdent(It.IsAny<BasicIdent>(), It.IsAny<TokenRequestBusiness>(), It.IsAny<tinyidp.infrastructure.bdd.Credential>(), It.IsAny<bool>())).Returns(true);
        _serviceProviderMock.Setup(x => x.GetRequiredKeyedService(It.IsAny<Type>(), It.IsAny<object?>())).Returns(_tokenStrategyMock.Object);
        _encryptionServiceMock.Setup(x => x.Encrypt(It.IsAny<string>())).Returns("lkjlkljkKLJLJ");

        _credentialBusinessMock.Setup(x => x.GetByIdent(It.IsAny<string>())).Returns(Task.FromResult<tinyidp.infrastructure.bdd.Credential?>(client));

        TokenResponseBusiness token =  await _tokenService.GetToken(_contextMock.Object, request);

        Assert.NotEmpty(token.access_token);
        Assert.NotEmpty(token.refresh_token);

    }
}
