using Microsoft.IdentityModel.Tokens;

namespace tinyidp.Controllers.Models;

public class KeysResponse
{
    public List<JsonWebKey> Keys { get; set; } = null!;
}