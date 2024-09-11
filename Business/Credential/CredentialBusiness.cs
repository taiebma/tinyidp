
using tinyidp.Business.BusinessEntities;
using tinyidp.Extensions;
using tinyidp.infrastructure.bdd;
using tinyidp.Exceptions;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace tinyidp.Business.Credential;

public class CredentialBusiness : ICredentialBusiness
{
    private readonly ILogger<CredentialBusiness> _logger;
    private readonly ICredentialRepository _credentialRepository;
    private readonly ICertificateRepository _certificateRepository;

    public CredentialBusiness(ILogger<CredentialBusiness> logger, ICredentialRepository repo, ICertificateRepository certificateRepository)
    {
        _logger = logger;
        _credentialRepository = repo;
        _certificateRepository = certificateRepository;
    }
    public void AddNewCredential(CredentialBusinessEntity entity)
    {
        entity.Pass = BCrypt.Net.BCrypt.EnhancedHashPassword(entity.Pass, 13);
        _credentialRepository.Add(entity.ToEntity());
    }

    public async Task<List<CredentialBusinessEntity>> GetAll()
    {
        var result = await _credentialRepository.GetAll();
        return result.Select(p => p.ToBusiness()).ToList();
    }

    public async Task<List<CredentialBusinessEntity>> SearchByState(int state)
    {
        var result = await _credentialRepository.SearchByState(state);
        return result.Select(p => p.ToBusiness()).ToList();
    }

    public async Task<List<CredentialBusinessEntity>> SearchByIdentLike(string ident)
    {
        var result = await _credentialRepository.SearchByIdentLike(ident);
        return result.Select(p => p.ToBusiness()).ToList();
    }

    public async Task<CredentialBusinessEntity?> GetByIdent(string ident)
    {
        var result = await _credentialRepository.GetByIdentReadOnly(ident);
        return result?.ToBusiness();
    }

    public async Task<CredentialBusinessEntity?> GetByAuthorizationCode(string code)
    {
        var result = await _credentialRepository.GetByAuthorizationCode(code);
        return result?.ToBusiness();
    }

    public async Task<CredentialBusinessEntity?> GetByRefreshToken(string token)
    {
        var result = await _credentialRepository.GetByRefreshToken(token);
        return result?.ToBusiness();
    }

    public CredentialBusinessEntity Get(int id)
    {
        var result =  _credentialRepository.GetByIdReadOnly(id);
        if (result == null)
            throw new Exception("Credential not found");
        return result.ToBusiness();
    }

    public CredentialBusinessEntity GetWithCertificates(int id)
    {
        var result =  _credentialRepository.GetWithCertificates(id);
        if (result == null)
            throw new Exception("Credential not found");
        return result.ToBusiness();
    }

    public void Update(CredentialBusinessEntity entity)
    {
        if (!entity.PassNew.Equals(entity.Pass))
            entity.Pass = BCrypt.Net.BCrypt.EnhancedHashPassword(entity.PassNew, 13);
        _credentialRepository.Update(entity.ToEntity());
        _credentialRepository.SaveChanges();
   }

    public void Remove(CredentialBusinessEntity entity)
    {
        _credentialRepository.Remove(entity.ToEntity());
    }

    public async Task<bool> VerifyPassword(string login, string pass)
    {
        tinyidp.infrastructure.bdd.Credential? entity = await _credentialRepository.GetByIdentReadOnly(login);
        if (entity == null)
        {
            return false;
        }
        bool result;
        try
        {
            result = BCrypt.Net.BCrypt.EnhancedVerify(pass, entity.Pass);
        }
        catch (Exception ex)
        {
            throw new TinyidpCredentialException("Error when verify password", ex);
        }
        return result;
    }

    public async Task<CredentialBusinessEntity> Authorize(HttpContext? httpContext, AuthorizationRequest request)
    {
        tinyidp.infrastructure.bdd.Credential? client;
        tinyidp.infrastructure.bdd.Credential? user;

        if (httpContext == null)
            throw new TinyidpCredentialException("No HTTP Context");
 
        if (string.IsNullOrEmpty(request.response_type) || request.response_type != "code")
           throw new TinyidpCredentialException("response_type must be code");
 
        if (string.IsNullOrEmpty(request.redirect_uri))
           throw new TinyidpCredentialException("No redirect URI");
 
        if ( (user = await IdentifyUserWithAuthorizeHeader(httpContext)) == null)
            user = await _credentialRepository.GetByIdentReadOnly(httpContext?.User?.Identity?.Name??String.Empty);

        if (user == null)
        {
            throw new TinyidpCredentialException("User unknown", "invalid_client");
        }

        client = await _credentialRepository.GetByIdentReadOnly(request.client_id);
        if (client == null)
        {
            throw new TinyidpCredentialException("Client id unknown", "invalid_client");
        }
        if (client.RoleIdent != (int)RoleCredential.Client)
            throw new TinyidpCredentialException("Only client role can use client_credential", "unsupported_grant_type");
        if (!(client.RedirectUri?.Equals(request.redirect_uri)??false))
            throw new TinyidpCredentialException("No redirect Uri parameter or redirect Uri different");
            
        CredentialBusinessEntity clientResp = GenerateCode(user, client, request.redirect_uri, request.scope, request.code_challenge, request.code_challenge_method);
        _credentialRepository.SaveChanges();
        return clientResp;
    }

    private async Task<tinyidp.infrastructure.bdd.Credential?> IdentifyUserWithAuthorizeHeader(HttpContext httpContext)
    {
        var authHeader = httpContext.Request.Headers["Authorization"].ToString();
        if (authHeader == null)
            return null;

        if (!authHeader.StartsWith("Basic", StringComparison.OrdinalIgnoreCase))
            return null;

        var parameters = authHeader.Substring("Basic ".Length);
        var authorizationKeys = Encoding.UTF8.GetString(Convert.FromBase64String(parameters));

        var authorizationResult = authorizationKeys.IndexOf(':');
        if (authorizationResult == -1)
            throw new TinyidpCredentialException("Basic Authorization must be <client_id>:<client_secret> format", "invalid_request");

        string clientId = authorizationKeys.Substring(0, authorizationResult);
        string clientSecret = authorizationKeys.Substring(authorizationResult + 1);

        if (!(await VerifyPassword(clientId, clientSecret)))
            throw new TinyidpCredentialException("Invalid client id or client secret", "invalid_request");

        tinyidp.infrastructure.bdd.Credential? user = await _credentialRepository.GetByIdentReadOnly(clientId);

        if (user != null)
        {
            if (user.RoleIdent != (int)RoleCredential.User)
                throw new TinyidpCredentialException("This type of user cannot obtain authorization code", "invalid_request");

            UpdateLastUserConnection(user);

            CreateIdentityCooky(user.ToBusiness(), httpContext);
        }

        return user;
    }

    public async void CreateIdentityCooky(CredentialBusinessEntity user, HttpContext httpContext)
    {

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Ident),
            new Claim("FullName", user.Ident),
            new Claim("Role", user.RoleIdent.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, 
            new ClaimsPrincipal(claimsIdentity));

    }

    private void UpdateLastUserConnection(tinyidp.infrastructure.bdd.Credential user)
    {
        user.LastIdent = DateTime.Now;
        _credentialRepository.Update(user);
    }

    public CredentialBusinessEntity GenerateCode(tinyidp.infrastructure.bdd.Credential user, tinyidp.infrastructure.bdd.Credential client, string redirectUri, string scope, string? code_challenge, string? code_challenge_method)
    {
        CredentialBusinessEntity clientResp = client.ToBusiness();

        if (client.RedirectUri == null)
            throw new TinyidpCredentialException("No redirect URL configured for this user");
        if (!client.RedirectUri.StartsWith("https"))
            throw new TinyidpCredentialException("Redirect URL must be HTTPs");

        if (!client.RedirectUri.Equals(redirectUri))
            throw new TinyidpCredentialException("Redirect URL does not match configuration");

        IEnumerable<string> scopes = new List<string>();
        if (clientResp.AllowedScopes != null)
        {
            scopes = clientResp.AllowedScopes.Intersect(scope.Split(' '));
            if (!scopes.Any())
            {
                throw new TinyidpCredentialException("No scope match", "invalid_scope");
            }
        }

        var rand = RandomNumberGenerator.Create();
        byte[] bytes = new byte[32];
        rand.GetBytes(bytes);
        var code = Base64UrlEncoder.Encode(bytes);

        client.CodeChallenge = code_challenge;
        client.CodeChallengeMethod = code_challenge_method;
        _credentialRepository.Update(client);

        user.AuthorizationCode = code;
        _credentialRepository.Update(user);

        clientResp = client.ToBusiness();
        clientResp.AuthorizationCode = code;
        return clientResp;
    }

    public void AddNewCertificate(CertificateBusinessEntity entity)
    {
        entity.CreationDate = DateTime.Now;
        _certificateRepository.Add(entity.ToEntity());
    }

    public async Task<CertificateBusinessEntity?> GetCertificate(int id)
    {
        var certif = await _certificateRepository.Get(id);
        return certif?.ToBusiness();
    }

    public void UpdateCertificate(CertificateBusinessEntity entity)
    {
        _certificateRepository.Update(entity.ToEntity());
    }
    public void RemoveCertificate(CertificateBusinessEntity entity)
    {
        _certificateRepository.Remove(entity.ToEntity());
    }

    public async Task<CredentialBusinessEntity?> GetCredentialByCertificate(string serial, string issuer)
    {
        var cred = await _credentialRepository.GetCredentialByCertificate(serial, issuer);
        return cred?.ToBusiness();
    }

}
