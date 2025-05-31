
using tinyidp.Business.BusinessEntities;
using tinyidp.infrastructure.bdd;

namespace tinyidp.Business.Credential;

public interface ICredentialBusiness
{
    public void AddNewCredential(CredentialBusinessEntity entity);
    public void Update(CredentialBusinessEntity entity);
    public void UpdateEntity(infrastructure.bdd.Credential entity);
    public void Remove(CredentialBusinessEntity entity);
    public Task<List<CredentialBusinessEntity>> GetAll();
    public Task<List<CredentialBusinessEntity>> SearchByState(int state);
    public Task<List<CredentialBusinessEntity>> SearchByIdentLike(string ident);
    public Task<infrastructure.bdd.Credential?> GetByIdent(string ident);
    public Task<infrastructure.bdd.Credential?> GetByAuthorizationCode(string code);
    public Task<infrastructure.bdd.Credential?> GetByRefreshToken(string token);
    public infrastructure.bdd.Credential Get(int id);
    public Task<bool> VerifyPassword(string login, string pass);
    public bool CheckPassword(string entityPass, string pass);
    public Task<CredentialBusinessEntity> Authorize(HttpContext? httpContext, AuthorizationRequest request);
    public void CreateIdentityCooky(CredentialBusinessEntity user, HttpContext httpContext);
    public void AddNewCertificate(CertificateBusinessEntity entity);
    public CredentialBusinessEntity GetWithCertificates(int id);
    public Task<CertificateBusinessEntity?> GetCertificate(int id);
    public void UpdateCertificate(CertificateBusinessEntity entity);
    public void RemoveCertificate(CertificateBusinessEntity entity);
    public Task<infrastructure.bdd.Credential?> GetCredentialByCertificate(string serial, string issuer);
    public AppUser GetUserInfo(HttpContext? context);
    
}