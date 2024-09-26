namespace tinyidp.Business.BusinessEntities;

public class TokenRequestBusiness
{
    public string? client_id { get; set; } = null!;
    public string? client_secret { get; set; } = null!;
    public string? code { get; set; } = null!;
    public string grant_type { get; set; } = null!;
    public string? redirect_uri { get; set; } = null!;
    public string? code_verifier { get; set; } = null!;
    public IList<string>? scope { get; set; } = null!;
    public string? device_code { get; set; } = null!;
    public string? refresh_token { get; set; } = null!;
}