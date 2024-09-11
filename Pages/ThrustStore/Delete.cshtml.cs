using tinyidp.Business.BusinessEntities;
using tinyidp.Business.Credential;
using tinyidp.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using tinyidp.Pages.Models;

namespace tinyidp.Pages.ThrustStore;

public class DeleteThrustStoreModel : PageModel
{
    private readonly ILogger<DeleteThrustStoreModel> _logger;
    private readonly IThrustStoreService _thrustStoreService;

    [BindProperty]
    public ThrustStoreEditModel? _thrustStoreEdit { get; set; }

    public DeleteThrustStoreModel(ILogger<DeleteThrustStoreModel> logger, IThrustStoreService thrustStoreService)
    {
        _thrustStoreService = thrustStoreService;
        _logger = logger;
    }

    public void OnGet(int id)
    {
        _thrustStoreEdit = _thrustStoreService.GetCa((int)id).Result?.ToModelEdit();

        if (_thrustStoreEdit == null)
        {
            _thrustStoreEdit =  new ThrustStoreEditModel();
            _thrustStoreEdit.ExceptionMessage = String.Format("Certificate {0} not found", id);
            _thrustStoreEdit.CanAccess = false;
        }
        
        if (User.Claims.Role() != RoleCredential.Admin)
        {
            _thrustStoreEdit.ExceptionMessage = "You don't have rights to access";
            _thrustStoreEdit.CanAccess = false;
        }
        else
        {
            _thrustStoreEdit.CanAccess = true;
        }
    }

    public IActionResult OnPost()
    {

        try
        {
            if (_thrustStoreEdit != null)
            {
                _thrustStoreService.RemoveCa(_thrustStoreEdit.ToBusiness());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            if (_thrustStoreEdit != null)
                _thrustStoreEdit.ExceptionMessage = String.Format("{0} - {1}", ex.Message, ex.InnerException?.Message);
            return Page();
        }
        return RedirectToPage("./View");
    }
}

