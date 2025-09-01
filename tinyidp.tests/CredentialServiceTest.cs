using System;
using System.Reflection;
using System.Runtime.Remoting;
using System.Security.Claims;
using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using tinyidp.Business.BusinessEntities;
using tinyidp.Business.Credential;
using tinyidp.Encryption;
using tinyidp.Exceptions;
using tinyidp.infrastructure.bdd;

namespace tinyidp.tests;

public class CredentialServiceTest
{
    private Mock<ILogger<CredentialBusiness>> _loggerMock;
    private Mock<ICredentialRepository> _credentialRepositoryMock;
    private Mock<ICertificateRepository> _certificateRepositoryMock;
    private Mock<HttpContext> _contextMock;
    private Mock<HttpRequest> _requestMock;
    private Mock<IHeaderDictionary> _headerMock;
    private IHashedPasswordPbkbf2 _hashedPassword;

    private Mock<CredentialBusiness> _credentialBusinessMock;
    private CredentialBusiness _credentialBusiness;

    private object? _credentialBusinessInstance; 
    private MethodInfo _methodIdentifyUserWithAuthorizeHeader;

    public CredentialServiceTest()
    {
        _loggerMock = new Mock<ILogger<CredentialBusiness>>();
        _certificateRepositoryMock = new Mock<ICertificateRepository>();
        _credentialRepositoryMock = new Mock<ICredentialRepository>();
        _contextMock = new Mock<HttpContext>();
        _requestMock = new Mock<HttpRequest>();
        _headerMock = new Mock<IHeaderDictionary>();
        _credentialBusinessMock = new Mock<CredentialBusiness>();
        _hashedPassword = new HashedPasswordPbkbf2();

        _credentialBusiness = new CredentialBusiness(
            _loggerMock.Object,
            _credentialRepositoryMock.Object,
            _certificateRepositoryMock.Object,
            _hashedPassword);

        Type type = typeof(CredentialBusiness);
        _credentialBusinessInstance = Activator.CreateInstance(
            type, 
            _loggerMock.Object,
            _credentialRepositoryMock.Object,
            _certificateRepositoryMock.Object,
            _hashedPassword);
        
        _methodIdentifyUserWithAuthorizeHeader = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => x.Name == "IdentifyUserWithAuthorizeHeader" && x.IsPrivate)
            .First();
    }

    [Fact]
    public async Task VerifyPassword_IdentFound_ResultTrue()
    {
        string login = "Test1";
        string password = "Test1Test1!";
        tinyidp.infrastructure.bdd.Credential ident = new Credential {
            Ident = login,
            Pass = "AQAAAAEAAYagAAAAEHLSUMSArukchcL6jzL1iKx6sDvXpy2VvI2Q99s81hMh5g846furpiG19NkbhFBisw==", 
            Id = 1
        };

        _credentialRepositoryMock.Setup(x => x.GetByIdentReadOnly(It.IsAny<string>())).Returns(Task.FromResult<Credential?>(ident));

        var result = await _credentialBusiness.VerifyPassword(login, password);
        
        Assert.True(result);

    }

    [Fact]
    public async Task VerifyPassword_IdentNotFound_ResultFalse()
    {
        string login = "Test2";
        string password = "Test1Test1!";
        _credentialRepositoryMock.Setup(x => x.GetByIdentReadOnly(It.IsAny<string>())).Returns(Task.FromResult<Credential?>(null));

        var result = await _credentialBusiness.VerifyPassword(login, password);
        
        Assert.False(result);

    }

    [Fact]
    public async Task VerifyPassword_IdentFound_ResultFalse()
    {
        string login = "Test1";
        string password = "Test1Test1";
        tinyidp.infrastructure.bdd.Credential ident = new Credential {
            Ident = login,
            Pass = "AQAAAAEAAYagAAAAEHLSUMSArukchcL6jzL1iKx6sDvXpy2VvI2Q99s81hMh5g846furpiG19NkbhFBisw==", 
            Id = 1
        };

        _credentialRepositoryMock.Setup(x => x.GetByIdentReadOnly(It.IsAny<string>())).Returns(Task.FromResult<Credential?>(ident));

        var result = await _credentialBusiness.VerifyPassword(login, password);
        
        Assert.False(result);

    }

    [Fact]
    public async Task Authorize_NoHttpContext_ResultException()
    {
        AuthorizationRequest authorizationRequest = new AuthorizationRequest();

        TinyidpCredentialException ex = await Assert.ThrowsAsync<TinyidpCredentialException>(async () =>  await _credentialBusiness.Authorize(null, authorizationRequest));
        Assert.Equal("No HTTP Context", ex.Message);
    }

    [Fact]
    public async Task Authorize_NoResponseType_ResultException()
    {
        AuthorizationRequest authorizationRequest = new AuthorizationRequest();
        _headerMock.Setup(x => x["Authorization"]).Returns("Basic VGVzdDk6VGVzdDlUZXN0OSE=");
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);

        TinyidpCredentialException ex = await Assert.ThrowsAsync<TinyidpCredentialException>(async () =>  await _credentialBusiness.Authorize(_contextMock.Object, authorizationRequest));
        Assert.Equal("response_type must be code", ex.Message);        
    }

    [Fact]
    public async Task Authorize_ResponseTypeNotCode_ResultException()
    {
        AuthorizationRequest authorizationRequest = new AuthorizationRequest {
            response_type = "", client_id = "Test1", scope = "scope1", redirect_uri = ""
        };
        _headerMock.Setup(x => x["Authorization"]).Returns("Basic VGVzdDk6VGVzdDlUZXN0OSE=");
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);

        TinyidpCredentialException ex = await Assert.ThrowsAsync<TinyidpCredentialException>(async () =>  await _credentialBusiness.Authorize(_contextMock.Object, authorizationRequest));
        Assert.Equal("response_type must be code", ex.Message);                
    }

    [Fact]
    public async Task Authorize_NoRedirectUri_ResultException()
    {
        AuthorizationRequest authorizationRequest = new AuthorizationRequest {
            response_type = "code", client_id = "Test1", scope = "scope1", redirect_uri = ""
        };
        _headerMock.Setup(x => x["Authorization"]).Returns("Basic VGVzdDk6VGVzdDlUZXN0OSE=");
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);

        TinyidpCredentialException ex = await Assert.ThrowsAsync<TinyidpCredentialException>(async () =>  await _credentialBusiness.Authorize(_contextMock.Object, authorizationRequest));
        Assert.Equal("No redirect URI", ex.Message);                
    }

    [Fact]
    public async Task Authorize_NoHeaderIdent_NoCookyIdent_ResultException()
    {
        AuthorizationRequest authorizationRequest = new AuthorizationRequest {
            response_type = "code", client_id = "Test1", scope = "scope1", redirect_uri = "http://localhost"
        };
//        _headerMock.Setup(x => x["Authorization"]).Returns("Basic VGVzdDk6VGVzdDlUZXN0OSE=");
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);

        TinyidpCredentialException ex = await Assert.ThrowsAsync<TinyidpCredentialException>(async () =>  await _credentialBusiness.Authorize(_contextMock.Object, authorizationRequest));
        Assert.Equal("invalid_client", ex.error_description);                        
        Assert.Equal("User unknown", ex.Message);                        
    }

    [Fact]
    public async Task Authorize_NoHeaderIdent_CookyIdent_ClientNotFound_ResultException()
    {
        AuthorizationRequest authorizationRequest = new AuthorizationRequest {
            response_type = "code", client_id = "Test1", scope = "scope1", redirect_uri = "http://localhost"
        };
        Credential? client = null;
        Credential? user = new Credential {
            Ident = "Test1"
        };

        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);
        _credentialRepositoryMock.SetupSequence(x => x.GetByIdentReadOnly(It.IsAny<string>()))
            .Returns(Task.FromResult<Credential?>(user))
            .Returns(Task.FromResult<Credential?>(client));

        TinyidpCredentialException ex = await Assert.ThrowsAsync<TinyidpCredentialException>(async () =>  await _credentialBusiness.Authorize(_contextMock.Object, authorizationRequest));
        Assert.Equal("invalid_client", ex.error_description);                        
        Assert.Equal("Client id unknown", ex.Message);                        
    }

    [Fact]
    public async Task Authorize_NoHeaderIdent_CookyIdent_BadRole_ResultException()
    {
        AuthorizationRequest authorizationRequest = new AuthorizationRequest {
            response_type = "code", client_id = "Test1", scope = "scope1", redirect_uri = "http://localhost"
        };
        Credential? client = new Credential {
            Ident = "Test9"
        };
        Credential? user = new Credential {
            Ident = "Test1"
        };
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);
        _credentialRepositoryMock.SetupSequence(x => x.GetByIdentReadOnly(It.IsAny<string>()))
            .Returns(Task.FromResult<Credential?>(user))
            .Returns(Task.FromResult<Credential?>(client));

        TinyidpCredentialException ex = await Assert.ThrowsAsync<TinyidpCredentialException>(async () =>  await _credentialBusiness.Authorize(_contextMock.Object, authorizationRequest));
        Assert.Equal("unsupported_grant_type", ex.error_description);                        
        Assert.Equal("Only client role can use client_credential", ex.Message);                        
    }

    [Fact]
    public async Task Authorize_NoHeaderIdent_CookyIdent_BadRedirectUri_ResultException()
    {
        AuthorizationRequest authorizationRequest = new AuthorizationRequest {
            response_type = "code", client_id = "Test1", scope = "scope1", redirect_uri = "http://localhost"
        };
        Credential? client = new Credential {
            Ident = "Test9", RoleIdent = (int)RoleCredential.Client
        };
        Credential? user = new Credential {
            Ident = "Test1"
        };
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);
        _credentialRepositoryMock.SetupSequence(x => x.GetByIdentReadOnly(It.IsAny<string>()))
            .Returns(Task.FromResult<Credential?>(user))
            .Returns(Task.FromResult<Credential?>(client));

        TinyidpCredentialException ex = await Assert.ThrowsAsync<TinyidpCredentialException>(async () =>  await _credentialBusiness.Authorize(_contextMock.Object, authorizationRequest));
        Assert.Equal("No redirect Uri parameter or redirect Uri different", ex.Message);                        
    }

    [Fact]
    public async Task Authorize_NoHeaderIdent_CookyIdent_ResultOk()
    {
        AuthorizationRequest authorizationRequest = new AuthorizationRequest {
            response_type = "code", client_id = "Test1", scope = "scope1", redirect_uri = "https://localhost"
        };
        Credential? client = new Credential {
            Ident = "Test9", RoleIdent = (int)RoleCredential.Client, RedirectUri = "https://localhost", AllowedScopes = "scope1"
        };
        Credential? user = new Credential {
            Ident = "Test1"
        };
        CredentialBusinessEntity response = new CredentialBusinessEntity();

        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);
        _credentialRepositoryMock.SetupSequence(x => x.GetByIdentReadOnly(It.IsAny<string>()))
            .Returns(Task.FromResult<Credential?>(user))
            .Returns(Task.FromResult<Credential?>(client));
        _credentialRepositoryMock.Setup(x => x.SaveChanges());
        _credentialRepositoryMock.Setup(x => x.Update(It.IsAny<Credential>()));
            
        response = await _credentialBusiness.Authorize(_contextMock.Object, authorizationRequest);

        Assert.NotNull(response);
    }

    [Fact]
    public async Task IdentifyUserWithAuthorizeHeader_NoAuthorizeHeader_ResultNull()
    {
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);


        //Act
        Task<tinyidp.infrastructure.bdd.Credential?>? responseTask = (Task<Credential?>?)_methodIdentifyUserWithAuthorizeHeader.Invoke(_credentialBusinessInstance, new object [] {_contextMock.Object});
        Assert.NotNull(responseTask);
        tinyidp.infrastructure.bdd.Credential? response = await responseTask;

        Assert.Null(response);
    }

    [Fact]
    public async Task IdentifyUserWithAuthorizeHeader_NoBasicAuthorizeHeader_ResultNull()
    {
        _headerMock.Setup(x => x["Authorization"]).Returns("VGVzdDk6VGVzdDlUZXN0OSE=");
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);

        //Act
        Task<tinyidp.infrastructure.bdd.Credential?>? responseTask = (Task<Credential?>?)_methodIdentifyUserWithAuthorizeHeader.Invoke(_credentialBusinessInstance, new object [] {_contextMock.Object});
        Assert.NotNull(responseTask);
        tinyidp.infrastructure.bdd.Credential? response = await responseTask;

        Assert.Null(response);
    }

    [Fact]
    public async void IdentifyUserWithAuthorizeHeader_MalformedBasicAuthorizeHeader_ResultException()
    {
        _headerMock.Setup(x => x["Authorization"]).Returns("Basic VGVzdDk=");
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);

        TinyidpCredentialException ex = await Assert.ThrowsAsync<TinyidpCredentialException>(
           async () => {
                Task<tinyidp.infrastructure.bdd.Credential?>? responseTask = (Task<tinyidp.infrastructure.bdd.Credential?>?)_methodIdentifyUserWithAuthorizeHeader.Invoke(_credentialBusinessInstance, new object [] {_contextMock.Object});
                Assert.NotNull(responseTask);
                tinyidp.infrastructure.bdd.Credential? response = await responseTask;

            });

        Assert.Equal("Basic Authorization must be <client_id>:<client_secret> format", ex.Message);
        Assert.Equal("invalid_request", ex.error_description);
    }

    [Fact]
    public async void IdentifyUserWithAuthorizeHeader_ClientNotFound_ResultException()
    {
        _headerMock.Setup(x => x["Authorization"]).Returns("Basic VGVzdDk6VGVzdDlUZXN0OSE=");
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);

        _credentialRepositoryMock.Setup(x => x.GetByIdentReadOnly(It.IsAny<string>())).Returns(Task.FromResult<Credential?>(null));

        TinyidpCredentialException ex = await Assert.ThrowsAsync<TinyidpCredentialException>(
           async () => {
                Task<tinyidp.infrastructure.bdd.Credential?>? responseTask = (Task<tinyidp.infrastructure.bdd.Credential?>?)_methodIdentifyUserWithAuthorizeHeader.Invoke(_credentialBusinessInstance, new object [] {_contextMock.Object});
                Assert.NotNull(responseTask);
                tinyidp.infrastructure.bdd.Credential? response = await responseTask;

            });

        Assert.Equal("Invalid client id or client secret", ex.Message);
        Assert.Equal("invalid_request", ex.error_description);
    }

    [Fact]
    public async void IdentifyUserWithAuthorizeHeader_InvalidClientRole_ResultException()
    {
        _headerMock.Setup(x => x["Authorization"]).Returns("Basic VGVzdDk6VGVzdDlUZXN0OSE=");
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);
        string login = "Test9";
        tinyidp.infrastructure.bdd.Credential ident = new Credential {
            Ident = login,
            Pass = "AQAAAAEAAYagAAAAEHLSUMSArukchcL6jzL1iKx6sDvXpy2VvI2Q99s81hMh5g846furpiG19NkbhFBisw==", 
            Id = 1
        };

        _credentialRepositoryMock.Setup(x => x.GetByIdentReadOnly(It.IsAny<string>())).Returns(Task.FromResult<Credential?>(ident));

        TinyidpCredentialException ex = await Assert.ThrowsAsync<TinyidpCredentialException>(
           async () => {
                Task<tinyidp.infrastructure.bdd.Credential?>? responseTask = (Task<tinyidp.infrastructure.bdd.Credential?>?)_methodIdentifyUserWithAuthorizeHeader.Invoke(_credentialBusinessInstance, new object [] {_contextMock.Object});
                Assert.NotNull(responseTask);
                tinyidp.infrastructure.bdd.Credential? response = await responseTask;

            });

        Assert.Equal("This type of user cannot obtain authorization code", ex.Message);
        Assert.Equal("invalid_request", ex.error_description);
    }

    [Fact]
    public async void IdentifyUserWithAuthorizeHeader_InvalidClientId_ResultException()
    {
        _headerMock.Setup(x => x["Authorization"]).Returns("Basic VGVzdDk6VGVzdDlUZXN0OSE=");
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);
        string login = "Test1";
        tinyidp.infrastructure.bdd.Credential ident = new Credential {
            Ident = login,
            Pass = "AQAAAAEAAYagAAAAEHLSUMSArukchcL6jzL1iKx6sDvXpy2VvI2Q99s81hMh5g846furpiG19NkbhFBisw==", 
            Id = 1,
            RoleIdent = (int)RoleCredential.User
        };

        _credentialRepositoryMock.Setup(x => x.GetByIdentReadOnly(It.IsAny<string>())).Returns(Task.FromResult<Credential?>(ident));

        TinyidpCredentialException ex = await Assert.ThrowsAsync<TinyidpCredentialException>(
           async () => {
                Task<tinyidp.infrastructure.bdd.Credential?>? responseTask = (Task<tinyidp.infrastructure.bdd.Credential?>?)_methodIdentifyUserWithAuthorizeHeader.Invoke(_credentialBusinessInstance, new object [] {_contextMock.Object});
                Assert.NotNull(responseTask);
                tinyidp.infrastructure.bdd.Credential? response = await responseTask;

            });

        Assert.Equal("Invalid client id or client secret", ex.Message);
        Assert.Equal("invalid_request", ex.error_description);
    }

    [Fact]
    public async void IdentifyUserWithAuthorizeHeader_ResultOk()
    {
        _headerMock.Setup(x => x["Authorization"]).Returns("Basic VGVzdDE6VGVzdDFUZXN0MSE=");
        _requestMock.Setup(x => x.Headers).Returns(_headerMock.Object);
        _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);
        Mock<IAuthenticationService> mockedIAuthenticationService = new();
        mockedIAuthenticationService
            .Setup(x => x.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string?>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()));
        Mock<IServiceProvider> mockedIServiceProvider = new();
        mockedIServiceProvider
            .Setup(x => x.GetService(typeof(IAuthenticationService)))
            .Returns(mockedIAuthenticationService.Object);
        Mock<HttpContext> mockedHttpContext = new Mock<HttpContext>();
        _contextMock.Setup(x => x.RequestServices).Returns(mockedIServiceProvider.Object);        
        string login = "Test1";
        tinyidp.infrastructure.bdd.Credential ident = new Credential {
            Ident = login,
            Pass = "AQAAAAEAAYagAAAAEHLSUMSArukchcL6jzL1iKx6sDvXpy2VvI2Q99s81hMh5g846furpiG19NkbhFBisw==", 
            Id = 1,
            RoleIdent = (int)RoleCredential.User
        };
        _credentialRepositoryMock.Setup(x => x.SaveChanges());
        _credentialRepositoryMock.Setup(x => x.Update(It.IsAny<Credential>()));

        _credentialRepositoryMock.Setup(x => x.GetByIdentReadOnly(It.IsAny<string>())).Returns(Task.FromResult<Credential?>(ident));

        
        Task<tinyidp.infrastructure.bdd.Credential?>? responseTask = (Task<tinyidp.infrastructure.bdd.Credential?>?)_methodIdentifyUserWithAuthorizeHeader.Invoke(_credentialBusinessInstance, new object [] {_contextMock.Object});
        Assert.NotNull(responseTask);
        tinyidp.infrastructure.bdd.Credential? response = await responseTask;

        Assert.NotNull(response);
    }


    [Fact]
    public void GenerateCode_NoRedirectUri_ResultException()
    {
        tinyidp.infrastructure.bdd.Credential? client = new tinyidp.infrastructure.bdd.Credential {
            Ident = "Test9", RoleIdent = (int)RoleCredential.Client
        };
        tinyidp.infrastructure.bdd.Credential? user = new tinyidp.infrastructure.bdd.Credential {
            Ident = "Test1"
        };
        string redirectUri = "https://localhost";
        string scope = "scope1";
        string code_challenge = null!;
        string code_challenge_method = null!;

        TinyidpCredentialException ex = Assert.Throws<TinyidpCredentialException>(
            () => 
                 _credentialBusiness.GenerateCode(user, client, redirectUri, scope, code_challenge, code_challenge_method, null)
            );

        Assert.Equal("No redirect URL configured for this user", ex.Message);
    }

    [Fact]
    public void GenerateCode_NotHttps_ResultException()
    {
        tinyidp.infrastructure.bdd.Credential? client = new tinyidp.infrastructure.bdd.Credential {
            Ident = "Test9", RoleIdent = (int)RoleCredential.Client, RedirectUri = "http://localhost", AllowedScopes = "scope1"
        };
        tinyidp.infrastructure.bdd.Credential? user = new tinyidp.infrastructure.bdd.Credential {
            Ident = "Test1"
        };
        string redirectUri = "https://localhost";
        string scope = "scope1";
        string code_challenge = null!;
        string code_challenge_method = null!;

        TinyidpCredentialException ex = Assert.Throws<TinyidpCredentialException>(
            () => 
                 _credentialBusiness.GenerateCode(user, client, redirectUri, scope, code_challenge, code_challenge_method, null)
            );

        Assert.Equal("Redirect URL must be HTTPs", ex.Message);
    }

    [Fact]
    public void GenerateCode_DifferentRedirectUri_ResultException()
    {
        tinyidp.infrastructure.bdd.Credential? client = new tinyidp.infrastructure.bdd.Credential {
            Ident = "Test9", RoleIdent = (int)RoleCredential.Client, RedirectUri = "https://localhost:8080", AllowedScopes = "scope1"
        };
        tinyidp.infrastructure.bdd.Credential? user = new tinyidp.infrastructure.bdd.Credential {
            Ident = "Test1"
        };
        string redirectUri = "https://localhost";
        string scope = "scope1";
        string code_challenge = null!;
        string code_challenge_method = null!;

        TinyidpCredentialException ex = Assert.Throws<TinyidpCredentialException>(
            () => 
                 _credentialBusiness.GenerateCode(user, client, redirectUri, scope, code_challenge, code_challenge_method, null)
            );

        Assert.Equal("Redirect URL does not match configuration", ex.Message);
    }

    [Fact]
    public void GenerateCode_NoScope_ResultException()
    {
        tinyidp.infrastructure.bdd.Credential? client = new tinyidp.infrastructure.bdd.Credential {
            Ident = "Test9", RoleIdent = (int)RoleCredential.Client, RedirectUri = "https://localhost"
        };
        tinyidp.infrastructure.bdd.Credential? user = new tinyidp.infrastructure.bdd.Credential {
            Ident = "Test1"
        };
        string redirectUri = "https://localhost";
        string scope = "scope1";
        string code_challenge = null!;
        string code_challenge_method = null!;

        TinyidpCredentialException ex = Assert.Throws<TinyidpCredentialException>(
            () => 
                 _credentialBusiness.GenerateCode(user, client, redirectUri, scope, code_challenge, code_challenge_method, null)
            );

        Assert.Equal("No scope match", ex.Message);
        Assert.Equal("invalid_scope", ex.error_description);
    }

}

