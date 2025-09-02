
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
using tinyidp.Encryption;
using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;
using tinyidp.Business.tokens;

namespace tinyidp.Business.Credential;

public class CredentialBusiness : ICredentialBusiness
{
    private readonly ILogger<CredentialBusiness> _logger;
    private readonly ICredentialRepository _credentialRepository;
    private readonly ICertificateRepository _certificateRepository;
    private readonly IHashedPasswordPbkbf2 _hashedPasswordPbkbf2;

    public CredentialBusiness(ILogger<CredentialBusiness> logger, ICredentialRepository repo, ICertificateRepository certificateRepository, IHashedPasswordPbkbf2 hashedPasswordPbkbf2)
    {
        _logger = logger;
        _credentialRepository = repo;
        _certificateRepository = certificateRepository;
        _hashedPasswordPbkbf2 = hashedPasswordPbkbf2;
    }
    public void AddNewCredential(CredentialBusinessEntity entity)
    {
//        entity.Pass = BCrypt.Net.BCrypt.EnhancedHashPassword(entity.Pass, 13);
        entity.Pass = _hashedPasswordPbkbf2.GetHashedPasswordPbkbf2(entity.Pass);
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

    public async Task<infrastructure.bdd.Credential?> GetByIdent(string ident)
    {
        var result = await _credentialRepository.GetByIdent(ident);
        return result;
    }

    public async Task<infrastructure.bdd.Credential?> GetByAuthorizationCode(string code)
    {
        var result = await _credentialRepository.GetByAuthorizationCode(code);
        return result;
    }

    public async Task<infrastructure.bdd.Credential?> GetByRefreshToken(string token)
    {
        var result = await _credentialRepository.GetByRefreshToken(token);
        return result;
    }

    public infrastructure.bdd.Credential Get(int id)
    {
        var result =  _credentialRepository.GetByIdReadOnly(id);
        if (result == null)
            throw new Exception("Credential not found");
        return result;
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
            entity.Pass = _hashedPasswordPbkbf2.GetHashedPasswordPbkbf2(entity.PassNew);
//            entity.Pass = BCrypt.Net.BCrypt.EnhancedHashPassword(entity.PassNew, 13);
        _credentialRepository.Update(entity.ToEntity());
        _credentialRepository.SaveChanges();
   }

    public void UpdateEntity(infrastructure.bdd.Credential entity)
    {
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
        return VerifyPassword(entity, pass);
    }

    private bool VerifyPassword(tinyidp.infrastructure.bdd.Credential entity, string pass)
    {
        bool result;
        try
        {
//            result = BCrypt.Net.BCrypt.EnhancedVerify(pass, entity.Pass);
            result = _hashedPasswordPbkbf2.VerifyHashedPasswordPbkbf2(entity.Pass, pass);
        }
        catch (Exception ex)
        {
            throw new TinyidpCredentialException("Error when verify password", ex);
        }
        return result;
    }

    public bool CheckPassword(string entityPass, string pass)
    {
        bool result;
        try
        {
//            result = BCrypt.Net.BCrypt.EnhancedVerify(pass, entity.Pass);
            result = _hashedPasswordPbkbf2.VerifyHashedPasswordPbkbf2(entityPass, pass);
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
 
        if (string.IsNullOrEmpty(request.response_type) || !request.response_type.Contains("code"))
           throw new TinyidpCredentialException("response_type must be code");
 
        if (string.IsNullOrEmpty(request.redirect_uri))
           throw new TinyidpCredentialException("No redirect URI");

        if (httpContext.User == null || httpContext.User?.Identity?.IsAuthenticated == false)
        {
            user = await IdentifyUserWithAuthorizeHeader(httpContext);
        }
        else
            user = await _credentialRepository.GetByIdentReadOnly(httpContext?.User?.Identity?.Name??String.Empty);
        if (user == null)
        {
            throw new TinyidpCredentialException("User unknown", "invalid_client");
        }

#pragma warning disable CS8604 // Null already test on top
        client = await _credentialRepository.GetByIdentReadOnly(request.client_id);
        if (client == null)
        {
            throw new TinyidpCredentialException("Client id unknown", "invalid_client");
        }
        if (client.RoleIdent != (int)RoleCredential.Client)
            throw new TinyidpCredentialException("Only client role can use client_credential", "unsupported_grant_type");
        if (!(client.RedirectUri?.Equals(request.redirect_uri)??false))
            throw new TinyidpCredentialException("No redirect Uri parameter or redirect Uri different");
            
        CredentialBusinessEntity clientResp = GenerateCode(
            user, 
            client, 
            request.redirect_uri, 
            request.scope, 
            request.code_challenge, 
            request.code_challenge_method,
            request.nonce);
        _credentialRepository.SaveChanges();
        return clientResp;
    }

    private async Task<tinyidp.infrastructure.bdd.Credential?> IdentifyUserWithAuthorizeHeader(HttpContext httpContext)
    {
        var authHeader = httpContext.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader))
            return null;

        if (!authHeader.StartsWith("Basic", StringComparison.OrdinalIgnoreCase))
            return null;

        var parameters = authHeader.Substring("Basic ".Length);
        var authorizationKeys = Encoding.UTF8.GetString(Convert.FromBase64String(parameters));

        var authorizationResult = authorizationKeys.IndexOf(':');
        if (authorizationResult == -1)
            throw new TinyidpCredentialException("Basic Authorization must be <client_id>:<client_secret> format", "invalid_request");

        string realUser = authorizationKeys.Substring(0, authorizationResult);
        string realUserPassword = authorizationKeys.Substring(authorizationResult + 1);

        if (!VerifyPassword(realUser, realUserPassword).Result)
            throw new TinyidpCredentialException("Invalid user or password", "invalid_request");

        tinyidp.infrastructure.bdd.Credential? user = await _credentialRepository.GetByIdentReadOnly(realUser);

        if (user != null)
        {
            if (user.RoleIdent != (int)RoleCredential.User)
                throw new TinyidpCredentialException("This type of user cannot obtain authorization code", "invalid_request");
        }

        UpdateLastUserConnection(user);

        CreateIdentityCooky(user.ToBusiness(), httpContext);

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

    public CredentialBusinessEntity GenerateCode(
        tinyidp.infrastructure.bdd.Credential user, 
        tinyidp.infrastructure.bdd.Credential client, 
        string redirectUri, 
        string scope, 
        string? code_challenge, 
        string? code_challenge_method,
        string? nonce)
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
            if (!string.IsNullOrEmpty(scope))
            {
                var allClientScopes = clientResp.AllowedScopes.Concat(TokenService.SupportedScopes).ToList();
                var tabScope = scope.Split(' ').ToList();
                if (tabScope.Where(p => !allClientScopes.Contains(p)).ToList().Count() > 0)
                {
                    throw new TinyidpTokenException("No scope match", "invalid_scope");
                }
                scopes = allClientScopes.Intersect(tabScope);
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
        user.Scoped = string.Join(' ', scopes);
        if (!string.IsNullOrEmpty(nonce))
        {
            user.Nonce = nonce;
        }
        _credentialRepository.Update(user);

        clientResp = client.ToBusiness();
        clientResp.AuthorizationCode = code;
        return clientResp;
    }

    public void AddNewCertificate(CertificateBusinessEntity entity)
    {
        entity.ValidityDate = DateTime.Now;
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

    public async Task<infrastructure.bdd.Credential?> GetCredentialByCertificate(string serial, string issuer)
    {
        var cred = await _credentialRepository.GetCredentialByCertificate(serial, issuer);
        return cred;
    }

    public AppUser GetUserInfo(HttpContext? context)
    {
        if (context == null)
        {
            throw new TinyidpCredentialException("No http context");
        }

        StringValues authorization = context.Request.Headers["Authorization"];
        if (authorization.Count == 0)
        {
            throw new TinyidpCredentialException("No bearer token");
        }
        string token = authorization.First()?.Substring("Bearer ".Length)??"";

        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(token);

        AppUser user = new AppUser() {
             sub = jwtSecurityToken.Claims.First(claim => claim.Type == "sub").Value,
             name = jwtSecurityToken.Claims.First(claim => claim.Type == "sub").Value
        };
        return user;
    }
}
