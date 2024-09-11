using tinyidp.Business.BusinessEntities;
using tinyidp.Business.Credential;
using tinyidp.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using tinyidp.Pages.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Cryptography.X509Certificates;

namespace tinyidp.Pages.Certificate;

public class CreateModel : PageModel
{
    private readonly ILogger<CreateModel> _logger;
    private readonly ICredentialBusiness _credentialBusiness;

    [BindProperty]
    public CertificateCreateModel? _credentialCreate { get; set; }


    public CreateModel(ILogger<CreateModel> logger, ICredentialBusiness credentialBusiness)
    {
        _credentialBusiness = credentialBusiness;
        _logger = logger;
    }

    public void OnGet(int id)
    {
        _credentialCreate = new CertificateCreateModel();
        _credentialCreate.IdClient = id;
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

    public IActionResult OnPost(List<IFormFile> files)
    {
/*        
        if (!ModelState.IsValid)
        {
            if (_credentialCreate != null)
                _credentialCreate.ExceptionMessage = "Fields invalids : " + string.Join(", ", 
                    ModelState.Where(p => p.Value?.ValidationState == ModelValidationState.Invalid)
                    .Select(p => p.Key.Split(".")[1]).ToList());
            return Page();
        }
*/
        if (_credentialCreate == null)
        {
            return Page();
        }
        if (files.Count == 0)
        {
            _credentialCreate.ExceptionMessage = "No file selected";
            return Page();
        }

        try
        {
            using MemoryStream memoryStream = new MemoryStream();
            files.First().CopyToAsync(memoryStream);
            X509Certificate2 cert = new X509Certificate2(memoryStream.ToArray());
            _credentialCreate.Dn = cert.Subject;
            DateTime.TryParse(cert.GetExpirationDateString(), out DateTime resultDateExp);
            _credentialCreate.ValidityDate = resultDateExp;
            _credentialCreate.Issuer = cert.Issuer;
            _credentialCreate.Serial = cert.SerialNumber;

            if (_credentialCreate != null)
                _credentialBusiness.AddNewCertificate(_credentialCreate.ToBusiness());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            if (_credentialCreate != null)
                _credentialCreate.ExceptionMessage = String.Format("{0} - {1}", ex.Message, ex.InnerException?.Message);
            return Page();
        }
        return RedirectToPage("/Credentials/Edit", new { id = _credentialCreate?.IdClient });
    }
}

