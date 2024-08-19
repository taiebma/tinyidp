using tinyidp.Business.BusinessEntities;
using tinyidp.Business.Credential;
using tinyidp.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using tinyidp.Pages.Models;

namespace tinyidp.Pages.Credentials;

public class CredentialsViewModel : PageModel
{
    private readonly ILogger<CredentialsViewModel> _logger;
    private readonly ICredentialBusiness _credentialBusiness;

    [BindProperty]
    public CredentialCreateModel? _credentialCreate { get; set; }

    [BindProperty]
    public List<CredentialView> _credentialView { get;set; }  = default!;

    [BindProperty(SupportsGet = true)]
    public string? SearchIdent { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SearchState { get; set; }

    public CredentialsViewModel(ILogger<CredentialsViewModel> logger, ICredentialBusiness credentialBusiness)
    {
        _credentialBusiness = credentialBusiness;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {

        List<CredentialBusinessEntity> credentials ;
        if (SearchState != null)
        {
            credentials = await _credentialBusiness.SearchByState(SearchState??1);
        }
        else
        {
            credentials = await _credentialBusiness.GetAll();
        }
        if (SearchIdent != null)
        {
            credentials = credentials.Where(x => x.Ident.Contains(SearchIdent)).ToList();
        }
        _credentialView = credentials.Select(p => p.ToModelView()).ToList();

        _credentialCreate = new CredentialCreateModel();
        if (User.Claims.Role() != RoleCredential.Admin && User.Claims.Role() != RoleCredential.User)
        {
            _credentialCreate.ExceptionMessage = "You don't have rights to access";
            _credentialCreate.CanAccess = false;
        }
        else
        {
            _credentialCreate.CanAccess = true;
        }

    }

}

