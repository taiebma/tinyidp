using Microsoft.AspNetCore.Http;
using tinyidp.Business.BusinessEntities;

namespace tinyidp.Business.tokens;

public interface ITokenStrategy
{
    static TokenTypeEnum Type {get;}
    TokenResponseBusiness GetTokenByType(TokenRequestBusiness request, infrastructure.bdd.Credential client);
    public bool VerifyClientIdent(BasicIdent ident, TokenRequestBusiness request, infrastructure.bdd.Credential client, bool checkPwd);
}