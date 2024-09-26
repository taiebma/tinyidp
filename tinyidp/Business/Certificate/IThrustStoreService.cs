
using System.Security.Cryptography.X509Certificates;
using tinyidp.Business.BusinessEntities;

public interface IThrustStoreService
{
    public Task<bool> VerifyWithChain(X509Certificate2 cert);
    public void AddCaToStore(string dn, string issuer, DateTime validityDate, X509Certificate2 certificate);

    public Task<List<ThrustStoreBusiness>> GetAllCaThrusted();

    public Task<ThrustStoreBusiness?> GetCa(int id);

    public void RemoveCa(ThrustStoreBusiness store);
}