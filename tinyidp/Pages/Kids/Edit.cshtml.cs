using tinyidp.Business.BusinessEntities;
using tinyidp.Business.Credential;
using tinyidp.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using tinyidp.Pages.Models;
using tinyidp.infrastructure.keysmanagment;

namespace tinyidp.Pages.Kids;

public class KidEdit : PageModel
{
    private readonly ILogger<KidEdit> _logger;
    private readonly IKeysManagment _keysManagment;

    [BindProperty]
    public KidEditModel? _kidEdit { get; set; }

    public KidEdit(ILogger<KidEdit> logger, IKeysManagment keysManagment)
    {
        _keysManagment = keysManagment;
        _logger = logger;
    }

    public void OnGet(int id)
    {
        KidBusinessEntity? result = _keysManagment.GetKeyById(id);
        if (result == null)
        {
            _kidEdit = new KidEditModel();
            _kidEdit.ExceptionMessage = "No KID found";
            return;
        }
        _kidEdit = result.ToModelEdit();

        if (User.Claims.Role() != RoleCredential.Admin)
        {
            _kidEdit.ExceptionMessage = "You don't have rights to access";
            _kidEdit.CanAccess = false;
            return;
        }
        else
        {
            _kidEdit.CanAccess = true;
        }
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            if (_kidEdit != null)
            {
                _keysManagment.Update(_kidEdit.ToBusiness());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            if (_kidEdit != null)
                _kidEdit.ExceptionMessage = String.Format("{0} - {1}", ex.Message, ex.InnerException?.Message);
            return Page();
        }
        return RedirectToPage("./View");
    }
}

