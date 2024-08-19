namespace tinyidp.Business.BusinessEntities;

public class RefreshTokenResponse
{
    public IEnumerable<string> Scopes { get; set; } = new List<string>();
    public IEnumerable<string> Audiences { get; set; } = new List<string>();
    public string? Ident { get; set; } = null!;
    
}