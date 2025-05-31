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
    
public class ChangePwdPage : PageModel
{
    private readonly ILogger<ChangePwdPage> _logger;
    private readonly ICredentialBusiness _credentialBusiness;


    [BindProperty]
    public ChangePwdModel _changePwdModel { get; set; } = null!;

    public ChangePwdPage(ILogger<ChangePwdPage> logger, ICredentialBusiness credentialBusiness)
    {
        _credentialBusiness = credentialBusiness;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(string ident)
    {
        int id;
        CredentialBusinessEntity? user;

        // Clear the existing external cookie
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (String.IsNullOrEmpty(ident))
            return RedirectToPage("/Index");

        if (int.TryParse(ident, out id))
        {
             user = _credentialBusiness.Get(id).ToBusiness();

        }
        else
        {
            user = await _credentialBusiness.GetByIdent(ident).ToBusinessAsync();
        }

        if (user == null)
        {
            return RedirectToPage("/Index");
        }
        if (!user.MustChangePwd)
            return RedirectToPage("/Index");

        _changePwdModel = user.ToModelChPwd();

        return Page();
    }

    public IActionResult OnPost()
    {
        if (ModelState.IsValid)
        {
            var user = _credentialBusiness.Get(_changePwdModel.Id).ToBusiness();
            if (!user.MustChangePwd)
            {
                _changePwdModel.ExceptionMessage = "Error you don't have right for doing this";
                return Page();
            }
            user.Pass = "";
            user.PassNew = _changePwdModel.PassNew;
            user.MustChangePwd = false;
            _credentialBusiness.Update(user);

            return RedirectToPage("/Account/Login");
        }

        // Something failed. Redisplay the form.
        return Page();
    }
}
