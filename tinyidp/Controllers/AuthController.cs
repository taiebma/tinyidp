using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using tinyidp.Business.Credential;
using tinyidp.Business.BusinessEntities;
using tinyidp.Extensions;

namespace tinyidp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ICredentialBusiness _credentialBusiness;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ICredentialBusiness credentialBusiness, ILogger<AuthController> logger)
    {
        _credentialBusiness = credentialBusiness;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Login et password sont obligatoires" });
        }

        try
        {
            // Vérifier le mot de passe
            if (!await _credentialBusiness.VerifyPassword(request.Login, request.Password))
            {
                _logger.LogWarning($"Failed login attempt for {request.Login}");
                return Unauthorized(new { message = "Identifiant ou mot de passe invalide" });
            }

            // Récupérer l'utilisateur
            var user = await _credentialBusiness.GetByIdent(request.Login);
            if (user == null)
            {
                return NotFound(new { message = "Utilisateur non trouvé" });
            }

            // Vérifier le rôle
            if (user.RoleIdent != (int)RoleCredential.Admin && user.RoleIdent != (int)RoleCredential.User)
            {
                return Forbid();
            }

            // Mettre à jour la dernière connexion
            user.LastIdent = DateTime.Now;
            _credentialBusiness.Update(user.ToBusiness());
            //await _credentialBusiness.SaveChanges();

            // Créer les claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Ident),
                new Claim("FullName", user.Ident),
                new Claim("Role", user.RoleIdent.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation($"User {request.Login} logged in successfully");

            return Ok(new { message = "Connexion réussie", redirectUrl = "/" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { message = "Erreur serveur" });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { message = "Déconnexion réussie" });
    }

    public class LoginRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
