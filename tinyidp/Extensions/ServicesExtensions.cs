
using tinyidp.Business.BusinessEntities;
using tinyidp.Business.tokens;

namespace tinyidp.Extensions;

public static class TinyidpServicesExtensions
{
   public static IServiceCollection AddTokenStrategies(this IServiceCollection services)
   {
        services.AddTransient<ITokenService, TokenService>();

        services.AddKeyedTransient<ITokenStrategy, RefreshToken>(TokenTypeEnum.refresh_token);
        services.AddKeyedTransient<ITokenStrategy, TokenAuthorizationCode>(TokenTypeEnum.code);
        services.AddKeyedTransient<ITokenStrategy, TokenAuthorizationCode>(TokenTypeEnum.authorization_code);
        services.AddKeyedTransient<ITokenStrategy, TokenClientCredential>(TokenTypeEnum.client_credential);

        return services;      
   }
}