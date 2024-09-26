
namespace tinyidp.infrastructure.bdd;

public interface ICertificateRepository
{
    public void Add(Certificate certificate);
    public void Remove(Certificate certificate);
    public void Update(Certificate certificate);
    public Task<Certificate?> Get(int id);

    public void AddThrustCertificate(ThrustStore certificate);
    public void RemoveThrustCertificate(ThrustStore certificate);
    public void UpdateThrustCertificate(ThrustStore certificate);
    public Task<ThrustStore?> GetThrustCertificate(int id);
    public Task<List<ThrustStore>> GetThrustStore();
}