using tinyidp.Business.BusinessEntities;
using tinyidp.Business.Credential;
using tinyidp.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using tinyidp.Pages.Models;

namespace tinyidp.Pages.ThrustStore;

public class ThrustStoreViewPage : PageModel
{
    private readonly ILogger<ThrustStoreViewPage> _logger;
    private readonly IThrustStoreService _thrustStoreService;

    [BindProperty]
    public ThrustStoreCreateModel _thrustStoreCreateModel { get; set;} = null!;
    [BindProperty]
    public List<ThrustStoreViewModel> _thrustStoreView { get;set; }  = default!;

    [BindProperty(SupportsGet = true)]
    public string? SearchIdent { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SearchState { get; set; }

    public ThrustStoreViewPage(ILogger<ThrustStoreViewPage> logger, IThrustStoreService thrustStoreService)
    {
        _thrustStoreService = thrustStoreService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {

        //credentials = await _thrustStoreService.GetAll();
        List<ThrustStoreBusiness> store = await _thrustStoreService.GetAllCaThrusted();
        _thrustStoreView = store.Select(p => p.ToModelView()).ToList();

        _thrustStoreCreateModel = new ThrustStoreCreateModel();
        if (User.Claims.Role() != RoleCredential.Admin && User.Claims.Role() != RoleCredential.User)
        {
            _thrustStoreCreateModel.ExceptionMessage = "You don't have rights to access";
            _thrustStoreCreateModel.CanAccess = false;
        }
        else
        {
            _thrustStoreCreateModel.CanAccess = true;
        }

    }

}

