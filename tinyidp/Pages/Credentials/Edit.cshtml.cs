using tinyidp.Business.BusinessEntities;
using tinyidp.Business.Credential;
using tinyidp.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using tinyidp.Pages.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace tinyidp.Pages.Credentials;

public class EditModel : PageModel
{
    private readonly ILogger<EditModel> _logger;
    private readonly ICredentialBusiness _credentialBusiness;

    [BindProperty]
    public CredentialEditModel? _credentialEdit { get; set; }

    public EditModel(ILogger<EditModel> logger, ICredentialBusiness credentialBusiness)
    {
        _credentialBusiness = credentialBusiness;
        _logger = logger;
    }

    public void OnGet(int id)
    {
        CredentialBusinessEntity result = _credentialBusiness.GetWithCertificates((int)id);
        _credentialEdit = result.ToModelEdit();
        
        if (User.Claims.Role() != RoleCredential.Admin)
        {
            _credentialEdit.ExceptionMessage = "You don't have rights to access";
            _credentialEdit.CanAccess = false;
        }
        else
        {
            _credentialEdit.CanAccess = true;
        }
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            if (_credentialEdit != null)
                _credentialEdit.ExceptionMessage = "Fields invalids : " + string.Join(", ", 
                    ModelState.Where(p => p.Value?.ValidationState == ModelValidationState.Invalid)
                    .Select(p => p.Key.Split(".")[1]).ToList());
            return Page();
        }

        try
        {
            if (_credentialEdit != null)
            {
                _credentialBusiness.Update(_credentialEdit.ToBusiness());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            if (_credentialEdit != null)
                _credentialEdit.ExceptionMessage = String.Format("{0} - {1}", ex.Message, ex.InnerException?.Message);
            return Page();
        }
        return RedirectToPage("./View");
    }
}

