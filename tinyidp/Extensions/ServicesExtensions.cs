
using tinyidp.Business.BusinessEntities;
using tinyidp.Business.tokens;

namespace tinyidp.Extensions;

public static class TinyidpServicesExtensions
{
   public static IServiceCollection AddTokenStrategies(this IServiceCollection services)
   {
       services.AddTransient<ITokenService, TokenService>();
      
       var tokenTypes = AppDomain.CurrentDomain.GetAssemblies()
           .SelectMany(assembly => assembly.GetTypes())
           .Where(type =>
               typeof(ITokenStrategy).IsAssignableFrom(type)
               && !type.IsInterface
               && !type.IsAbstract);

       foreach (var tokenType in tokenTypes)
       {
           TokenTypeEnum type =
               (TokenTypeEnum) tokenType!
                   .GetProperty("Type")!
                   .GetValue(null, null)!;
           services.AddKeyedTransient(typeof(ITokenStrategy), type, tokenType);
       }
      
       return services;
   }
}