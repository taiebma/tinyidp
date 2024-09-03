using Microsoft.AspNetCore.Http;
using tinyidp.Business.BusinessEntities;

namespace tinyidp.Business.tokens;

public interface ITokenStrategy
{
    static TokenTypeEnum Type {get;}
    TokenResponseBusiness GetTokenByType(TokenRequestBusiness request, CredentialBusinessEntity client);
    public Task<bool> VerifyClientIdent(BasicIdent ident, TokenRequestBusiness request, CredentialBusinessEntity client);
}