namespace tinyidp.Controllers.Models;

public class AuthorizeResponse
{
    /// <summary>
    /// code or implicit grant or client creditional 
    /// </summary>
    public string ResponseType { get; set; } = null!;
    public string Code { get; set; } = null!;
    /// <summary>
    /// required if it was present in the client authorization request
    /// </summary>
    public string State { get; set; } = null!;
    public string RedirectUri { get; set; } = null!;
    public IList<string> RequestedScopes { get; set; } = null!;
    public string GrantType { get; set; } = null!;
    public string Error { get; set; } = string.Empty;
    public string ErrorUri { get; set; } = null!;
    public string ErrorDescription { get; set; } = null!;
    public bool HasError => !string.IsNullOrEmpty(Error);
}