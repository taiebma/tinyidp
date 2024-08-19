using Microsoft.AspNetCore.Http;
using tinyidp.Business.BusinessEntities;

namespace tinyidp.Business.tokens;

public interface ITokenStrategy
{
    static TokenTypeEnum Type {get;}
    Task<TokenResponseBusiness> GetTokenByType(HttpContext httpContext, TokenRequestBusiness request, CredentialBusinessEntity client);
}