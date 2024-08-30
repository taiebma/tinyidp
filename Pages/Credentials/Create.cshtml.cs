using tinyidp.Business.BusinessEntities;
using tinyidp.Business.Credential;
using tinyidp.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using tinyidp.Pages.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace tinyidp.Pages.Credentials;

public class CreateModel : PageModel
{
    private readonly ILogger<CreateModel> _logger;
    private readonly ICredentialBusiness _credentialBusiness;

    [BindProperty]
    public CredentialCreateModel? _credentialCreate { get; set; }


    public CreateModel(ILogger<CreateModel> logger, ICredentialBusiness credentialBusiness)
    {
        _credentialBusiness = credentialBusiness;
        _logger = logger;
    }

    public void OnGet()
    {
        _credentialCreate = new CredentialCreateModel();
        if (User.Claims.Role() != RoleCredential.Admin)
        {
            _credentialCreate.ExceptionMessage = "You don't have rights to access";
            _credentialCreate.CanAccess = false;
        }
        else
        {
            _credentialCreate.CanAccess = true;
        }

    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            if (_credentialCreate != null)
                _credentialCreate.ExceptionMessage = "Fields invalids : " + string.Join(", ", 
                    ModelState.Where(p => p.Value?.ValidationState == ModelValidationState.Invalid)
                    .Select(p => p.Key.Split(".")[1]).ToList());
            return Page();
        }

        try
        {
            if (_credentialCreate != null)
                _credentialBusiness.AddNewCredential(_credentialCreate.ToBusiness());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            if (_credentialCreate != null)
                _credentialCreate.ExceptionMessage = String.Format("{0} - {1}", ex.Message, ex.InnerException?.Message);
            return Page();
        }
        return RedirectToPage("./View");
    }
}

