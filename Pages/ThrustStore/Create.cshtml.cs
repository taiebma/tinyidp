using tinyidp.Business.BusinessEntities;
using tinyidp.Business.Credential;
using tinyidp.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using tinyidp.Pages.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Components;
using System.Security.Cryptography.X509Certificates;

namespace tinyidp.Pages.ThrustStore;

public class CreateModel : PageModel
{
    private readonly ILogger<CreateModel> _logger;
    private readonly IThrustStoreService _thrustStoreService;

    [BindProperty]
    public ThrustStoreCreateModel? _thrustStoreCreate { get; set; }

    public CreateModel(ILogger<CreateModel> logger, IThrustStoreService thrustStoreService)
    {
        _thrustStoreService = thrustStoreService;
        _logger = logger;
    }

    public void OnGet(int id)
    {
        _thrustStoreCreate = new ThrustStoreCreateModel();
        if (User.Claims.Role() != RoleCredential.Admin)
        {
            _thrustStoreCreate.ExceptionMessage = "You don't have rights to access";
            _thrustStoreCreate.CanAccess = false;
        }
        else
        {
            _thrustStoreCreate.CanAccess = true;
        }

    }

    public IActionResult OnPost(List<IFormFile> files)
    {
        if (files.Count == 0)
        {
            _thrustStoreCreate = new ThrustStoreCreateModel();
            _thrustStoreCreate.ExceptionMessage = "No file selected";
            return Page();
        }
        using MemoryStream memoryStream = new MemoryStream();
        files.First().CopyToAsync(memoryStream);
        X509Certificate2 cert = new X509Certificate2(memoryStream.ToArray());

        try
        {
            DateTime.TryParse(cert.GetExpirationDateString(), out DateTime validityDate);
            _thrustStoreService.AddCaToStore(cert.Subject, cert.Issuer, validityDate, cert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            if (_thrustStoreCreate != null)
                _thrustStoreCreate.ExceptionMessage = String.Format("{0} - {1}", ex.Message, ex.InnerException?.Message);
            return Page();
        }
        return RedirectToPage("/ThrustStore/View");
    }
}

