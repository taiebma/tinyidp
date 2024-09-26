using tinyidp.Business.BusinessEntities;
using tinyidp.Business.Credential;
using tinyidp.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using tinyidp.Pages.Models;

namespace tinyidp.Pages.Certificate;

public class DeleteModel : PageModel
{
    private readonly ILogger<DeleteModel> _logger;
    private readonly ICredentialBusiness _credentialBusiness;

    [BindProperty]
    public CertificateEditModel? _credentialDelete { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchIdent { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SearchState { get; set; }

    public DeleteModel(ILogger<DeleteModel> logger, ICredentialBusiness credentialBusiness)
    {
        _credentialBusiness = credentialBusiness;
        _logger = logger;
    }

    public void OnGet(int id)
    {
        CertificateBusinessEntity? result = _credentialBusiness.GetCertificate((int)id).Result;
        _credentialDelete = result?.ToModelEdit()??new CertificateEditModel();
        if (result == null)
        {
            _credentialDelete.ExceptionMessage = String.Format("Certificate {0} not found", id);
            _credentialDelete.CanAccess = false;
        }
        
        if (User.Claims.Role() != RoleCredential.Admin)
        {
            _credentialDelete.ExceptionMessage = "You don't have rights to access";
            _credentialDelete.CanAccess = false;
        }
        else
        {
            _credentialDelete.CanAccess = true;
        }
    }

    public IActionResult OnPost()
    {

        try
        {
            if (_credentialDelete != null)
            {
                _credentialBusiness.RemoveCertificate(_credentialDelete.ToBusiness());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            if (_credentialDelete != null)
                _credentialDelete.ExceptionMessage = String.Format("{0} - {1}", ex.Message, ex.InnerException?.Message);
            return Page();
        }
        return RedirectToPage("/Credentials/Edit", new { id = _credentialDelete?.IdClient });
    }
}

