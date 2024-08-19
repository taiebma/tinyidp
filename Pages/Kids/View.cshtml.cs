using tinyidp.Business.BusinessEntities;
using tinyidp.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using tinyidp.Pages.Models;
using tinyidp.infrastructure.keysmanagment;

namespace tinyidp.Pages.Kids;

public class KidsViewModel : PageModel
{
    private readonly ILogger<KidsViewModel> _logger;
    private readonly IKeysManagment _keyManagment;

    [BindProperty]
    public List<KidView> _kidsView { get;set; }  = default!;

    [BindProperty]
    public string ErrorMessage { get; set; } = null!;

    [BindProperty]
    public bool CanAccess { get; set; } = false;

    public KidsViewModel(ILogger<KidsViewModel> logger, IKeysManagment keysManagment)
    {
        _logger = logger;
        _keyManagment = keysManagment;
    }

    public void OnGet()
    {
        if (User.Claims.Role() != RoleCredential.Admin)
        {
            ErrorMessage = "You don't have rights to access";
            CanAccess = false;
        }
        else
        {
            CanAccess = true;
            List<KidBusinessEntity> kids = _keyManagment.GetKeys();
            _kidsView = kids.Select(k => k.ToModelView()).ToList();
        }

    }

}

