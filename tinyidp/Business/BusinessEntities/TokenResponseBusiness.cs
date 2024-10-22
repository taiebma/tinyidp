using tinyidp.Extensions;
using tinyidp.Controllers.Enums;

namespace tinyidp.Business.BusinessEntities;

public class TokenResponseBusiness
{
    /// <summary>
    /// Oauth 2
    /// </summary>
    public string access_token { get; set; } = null!;

    /// <summary>
    /// OpenId Connect
    /// </summary>
    public string id_token { get; set; } = null!;

    /// <summary>
    /// By default is Bearer
    /// </summary>

    public string token_type { get; set; } = null!;
    public string nonce { get; set; } = null!;

    /// <summary>
    /// Authorization Code. This is always returned when using the Hybrid Flow.
    /// </summary>
    public string code { get; set; } = null!;

    public string refresh_token { get; set; } = null!;

    public RefreshTokenResponse refreshTokenResponse{ get; set; } = null!;

}