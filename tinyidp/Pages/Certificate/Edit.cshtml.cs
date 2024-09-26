using tinyidp.Business.BusinessEntities;
using tinyidp.Business.Credential;
using tinyidp.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using tinyidp.Pages.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace tinyidp.Pages.Certificate;

public class EditModel : PageModel
{
    private readonly ILogger<CreateModel> _logger;
    private readonly ICredentialBusiness _credentialBusiness;

    [BindProperty]
    public CertificateEditModel? _credentialEdit { get; set; }


    public EditModel(ILogger<CreateModel> logger, ICredentialBusiness credentialBusiness)
    {
        _credentialBusiness = credentialBusiness;
        _logger = logger;
    }

    public void OnGet(int id)
    {
        
        _credentialEdit = _credentialBusiness.GetCertificate(id).Result?.ToModelEdit();
        if (_credentialEdit == null)
        {
            _credentialEdit = new CertificateEditModel();
            _credentialEdit.Id = id;
        }
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
                _credentialBusiness.UpdateCertificate(_credentialEdit.ToBusiness());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            if (_credentialEdit != null)
                _credentialEdit.ExceptionMessage = String.Format("{0} - {1}", ex.Message, ex.InnerException?.Message);
            return Page();
        }
        return RedirectToPage("/Credentials/Edit", new { id = _credentialEdit?.IdClient });
    }
}

