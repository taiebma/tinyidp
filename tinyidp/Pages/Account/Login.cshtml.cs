using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using tinyidp.Pages.Models;
using tinyidp.Business.Credential;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using tinyidp.Business.BusinessEntities;
using tinyidp.Extensions;

namespace tinyidp.Pages.Account;
    
public class LoginPage : PageModel
{
    private readonly ILogger<LoginPage> _logger;
    private readonly ICredentialBusiness _credentialBusiness;


    [BindProperty]
    public LoginModel _input { get; set; } = null!;

    public string ReturnUrl { get; private set; } = null!;

    public LoginPage(ILogger<LoginPage> logger, ICredentialBusiness credentialBusiness)
    {
        _credentialBusiness = credentialBusiness;
        _logger = logger;
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (_input != null && !string.IsNullOrEmpty(_input.ExceptionMessage))
        {
            ModelState.AddModelError(string.Empty, _input.ExceptionMessage);
        }

        // Clear the existing external cookie
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        ReturnUrl = returnUrl??"/Index";
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl??"/Index";

        if (ModelState.IsValid)
        {
            var user = await AuthenticateUser(_input.Login, _input.Password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            //  Update last connection
            user.LastIdent = DateTime.Now;
            _credentialBusiness.Update(user);

            if (user.RoleIdent != RoleCredential.Admin && user.RoleIdent != RoleCredential.User)
            {
                ModelState.AddModelError(string.Empty, "This type of user cannot logged in");
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Ident),
                new Claim("FullName", user.Ident),
                new Claim("Role", user.RoleIdent.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity));

            return LocalRedirect(Url.GetLocalUrl(returnUrl));
        }

        // Something failed. Redisplay the form.
        return Page();
    }

    private async Task<CredentialBusinessEntity?> AuthenticateUser(string login, string password)
    {

        if (await _credentialBusiness.VerifyPassword(login, password))
        {
            CredentialBusinessEntity? ident  = await  _credentialBusiness.GetByIdent(login);
            return ident;
        }
        else
        {
            return null;
        }
    }
}
