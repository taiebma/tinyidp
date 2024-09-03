
namespace tinyidp.infrastructure.bdd;

public interface ICredentialRepository
{
    public void Add(Credential Credential);
    public void Remove(Credential Credential);
    public void Update(Credential Credential);
    public Credential? GetById(int id);
    public Task<Credential?> GetByIdent(string ident);
    public Credential? GetByIdReadOnly(int id);
    public Credential? GetWithCertificates(int id);
    public Task<Credential?> GetByIdentReadOnly(string ident);
    public Task<List<Credential>> SearchByIdentLike(string ident);
    public Task<Credential?> GetByAuthorizationCode(string code);
    public Task<Credential?> GetByRefreshToken(string token);
    public Task<List<Credential>> SearchByState(int state );
    public Task<List<Credential>> GetAll( );
    public void SaveChanges();
}