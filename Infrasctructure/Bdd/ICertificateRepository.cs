
namespace tinyidp.infrastructure.bdd;

public interface ICertificateRepository
{
    public void Add(Certificate certificate);
    public void Remove(Certificate certificate);
    public void Update(Certificate certificate);
    public Task<Certificate?> Get(int id);
}