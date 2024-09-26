using tinyidp.Extensions;
using tinyidp.Controllers.Enums;

namespace tinyidp.Controllers.Models;

public class TokenResponse
{
    /// <summary>
    /// Oauth 2
    /// </summary>
    public string access_token { get; set; } = null!;

    /// <summary>
    /// By default is Bearer
    /// </summary>

    public string token_type { get; set; } = TokenTypeEnum.Bearer.GetEnumDescription();

    /// <summary>
    /// Authorization Code. This is always returned when using the Hybrid Flow.
    /// </summary>
    public string code { get; set; } = null!;

    public string refresh_token { get; set; } = null!;
    
    /// <summary>
    /// For Error Details if any
    /// </summary>
    public string Error { get; set; } = string.Empty;
    public string error_description { get; set; } = string.Empty;
    public bool HasError => !string.IsNullOrEmpty(Error);
}