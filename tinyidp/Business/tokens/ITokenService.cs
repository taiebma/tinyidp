using Microsoft.AspNetCore.Http;
using tinyidp.Business.BusinessEntities;

namespace tinyidp.Business.tokens;

public interface ITokenService
{
    public Task<TokenResponseBusiness> GetToken(HttpContext? httpContext, TokenRequestBusiness request);
    
}