using tinyidp.Business.BusinessEntities;
using tinyidp.Business.Credential;
using tinyidp.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using tinyidp.Pages.Models;
using tinyidp.infrastructure.keysmanagment;

namespace tinyidp.Pages.Kids;

public class CreateKidModel : PageModel
{
    private readonly ILogger<CreateKidModel> _logger;
    private readonly IKeysManagment _keysManagment;

    [BindProperty]
    public KidCreateModel? _kidCreate { get; set; }


    public CreateKidModel(ILogger<CreateKidModel> logger, IKeysManagment keysManagment)
    {
        _logger = logger;
        _keysManagment = keysManagment;
    }

    public void OnGet()
    {
        _kidCreate = new KidCreateModel();
        if (User.Claims.Role() != RoleCredential.Admin)
        {
            _kidCreate.ExceptionMessage = "You don't have rights to access";
            _kidCreate.CanAccess = false;
        }
        else
        {
            _kidCreate.CanAccess = true;
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
            if (_kidCreate != null)
                _keysManagment.GenNewKey(_kidCreate.Algo, _kidCreate?.Kid??string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            if (_kidCreate != null)
                _kidCreate.ExceptionMessage = String.Format("{0} - {1}", ex.Message, ex.InnerException?.Message);
            return Page();
        }
        return RedirectToPage("./View");
    }
}

