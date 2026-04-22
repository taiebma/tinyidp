using System.Text.Json.Serialization;
using tinyidp.Business.BusinessEntities;
using tinyidp.Controllers.Models;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(TokenResponseBusiness))]
[JsonSerializable(typeof(RefreshTokenResponse))]
[JsonSerializable(typeof(TokenRequestBusiness))]
[JsonSerializable(typeof(DiscoveryResponse))]
[JsonSerializable(typeof(KeysResponse))]
[JsonSerializable(typeof(tinyidp.Business.BusinessEntities.AppUser))]
public partial class TinyIdpJsonSerializerContext : JsonSerializerContext
{
}