
namespace tinyidp.infrastructure.bdd;

public interface ICertificateRepository
{
    public void Add(Certificate certificate);
    public void Remove(Certificate certificate);
    public void Update(Certificate certificate);
    public Task<Certificate?> Get(int id);

    public void AddTrustCertificate(TrustStore certificate);
    public void RemoveTrustCertificate(TrustStore certificate);
    public void UpdateTrustCertificate(TrustStore certificate);
    public Task<TrustStore?> GetTrustCertificate(int id);
    public Task<List<TrustStore>> GetTrustStore();
}