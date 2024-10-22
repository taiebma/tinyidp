using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using tinyidp.Pages.Models;
using tinyidp.Business.Credential;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using tinyidp.Business.BusinessEntities;
using tinyidp.Exceptions;
using System.Text.Encodings.Web;
using System.Web;
using System.Text;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace tinyidp.Pages.Account;
    
public class IdentPage : PageModel
{
    private readonly ILogger<IdentPage> _logger;
    private readonly ICredentialBusiness _credentialBusiness;
    private readonly IActionContextAccessor _accessor;


    [BindProperty]
    public LoginModel _input { get; set; } = null!;

    [BindProperty]
    public string? _scope { get; set; } = null!;

    [BindProperty]
    public string? _state { get; set; } = null!;

    [BindProperty]
    public string? _redirectUri { get; set; } = null!;

    [BindProperty]
    public string? _client_id { get; set; } = null!;

    [BindProperty]
    public string? _code_challenge { get; set; } = null!;

    [BindProperty]
    public string? _code_challenge_method { get; set; } = null!;

    [BindProperty]
    public string? _nonce { get; set; } = null!;

    public IdentPage(ILogger<IdentPage> logger, ICredentialBusiness credentialBusiness, IActionContextAccessor accessor)
    {
        _credentialBusiness = credentialBusiness;
        _logger = logger;
        _accessor = accessor;
    }

    public void OnGet(string scope, string state, string redirect_uri, string client_id, string code_challenge, string code_challenge_method, string? nonce)
    {
        _scope = scope;
        _state = state;
        _redirectUri = redirect_uri;
        _client_id = client_id;
        _code_challenge = code_challenge;
        _code_challenge_method = code_challenge_method;
        _nonce = nonce;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {

        if (ModelState.IsValid)
        {
            string url = String.Format("/oauth/authorize?response_type=code&redirect_uri={0}&scope={1}&state={2}&client_id={3}&code_challenge={4}&code_challenge_method={5}&nonce={6}",
                HttpUtility.UrlEncode( _redirectUri), _scope, _state, _client_id, _code_challenge, _code_challenge_method, _nonce);

            if (!(await _credentialBusiness.VerifyPassword(_input.Login, _input.Password)))
            {
                _input.ExceptionMessage = "Invalid client id or client secret";
                return Page();
            }

            CredentialBusinessEntity? user = await _credentialBusiness.GetByIdent(_input.Login);
            if (user == null)
            {
                _input.ExceptionMessage = "User does not exists";
                return Page();
            }
            
            _credentialBusiness.CreateIdentityCooky(user, HttpContext);

            return Redirect(url);
        }

        // Something failed. Redisplay the form.
        return Page();
    }

    private async Task<CredentialBusinessEntity?> AuthenticateUser(string login, string password)
    {

        if (await _credentialBusiness.VerifyPassword(login, password))
        {
            CredentialBusinessEntity? ident  = await  _credentialBusiness.GetByIdent(login);
            return ident;
        }
        else
        {
            return null;
        }
    }
}
