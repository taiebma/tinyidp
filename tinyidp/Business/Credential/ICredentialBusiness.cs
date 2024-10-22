
using tinyidp.Business.BusinessEntities;

namespace tinyidp.Business.Credential;

public interface ICredentialBusiness
{
    public void AddNewCredential(CredentialBusinessEntity entity);
    public void Update(CredentialBusinessEntity entity);
    public void Remove(CredentialBusinessEntity entity);
    public Task<List<CredentialBusinessEntity>> GetAll();
    public Task<List<CredentialBusinessEntity>> SearchByState(int state);
    public Task<List<CredentialBusinessEntity>> SearchByIdentLike(string ident);
    public Task<CredentialBusinessEntity?> GetByIdent(string ident);
    public Task<CredentialBusinessEntity?> GetByAuthorizationCode(string code);
    public Task<CredentialBusinessEntity?> GetByRefreshToken(string token);
    public CredentialBusinessEntity Get(int id);
    public Task<bool> VerifyPassword(string login, string pass);
    public bool CheckPassword(string entityPass, string pass);
    public Task<CredentialBusinessEntity> Authorize(HttpContext? httpContext, AuthorizationRequest request);
    public void CreateIdentityCooky(CredentialBusinessEntity user, HttpContext httpContext);
    public void AddNewCertificate(CertificateBusinessEntity entity);
    public CredentialBusinessEntity GetWithCertificates(int id);
    public Task<CertificateBusinessEntity?> GetCertificate(int id);
    public void UpdateCertificate(CertificateBusinessEntity entity);
    public void RemoveCertificate(CertificateBusinessEntity entity);
    public Task<CredentialBusinessEntity?> GetCredentialByCertificate(string serial, string issuer);
    public AppUser GetUserInfo(HttpContext? context);
    
}