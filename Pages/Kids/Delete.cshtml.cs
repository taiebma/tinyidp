using tinyidp.Business.BusinessEntities;
using tinyidp.Business.Credential;
using tinyidp.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using tinyidp.Pages.Models;
using tinyidp.infrastructure.keysmanagment;

namespace tinyidp.Pages.Kids;

public class DeleteKid : PageModel
{
    private readonly ILogger<DeleteKid> _logger;
    private readonly IKeysManagment _keysManagment;

    [BindProperty]
    public KidDeleteModel? _kidDelete { get; set; }

    public DeleteKid(ILogger<DeleteKid> logger, IKeysManagment keysManagment)
    {
        _keysManagment = keysManagment;        
        _logger = logger;
    }

    public void OnGet(int id)
    {
        KidBusinessEntity? result = _keysManagment.GetKeyById(id);
        if (result == null)
        {
            _kidDelete = new KidDeleteModel();
            _kidDelete.ExceptionMessage = "No KID found";
            return;
        }
        _kidDelete = result.ToModelDelete();

        if (User.Claims.Role() != RoleCredential.Admin)
        {
            _kidDelete.ExceptionMessage = "You don't have rights to access";
            _kidDelete.CanAccess = false;
            return;
        }
        else
        {
            _kidDelete.CanAccess = true;
        }
}

    public IActionResult OnPost()
    {

        try
        {
            if (_kidDelete != null)
            {
                _keysManagment.Remove(_kidDelete.ToBusiness());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            if (_kidDelete != null)
                _kidDelete.ExceptionMessage = String.Format("{0} - {1}", ex.Message, ex.InnerException?.Message);
            return Page();
        }
        return RedirectToPage("./View");
    }
}

