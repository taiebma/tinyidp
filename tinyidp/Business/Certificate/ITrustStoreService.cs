
using System.Security.Cryptography.X509Certificates;
using tinyidp.Business.BusinessEntities;

public interface ITrustStoreService
{
    public Task<bool> VerifyWithChain(X509Certificate2 cert);
    public void AddCaToStore(string dn, string issuer, DateTime validityDate, X509Certificate2 certificate);

    public Task<List<TrustStoreBusiness>> GetAllCaTrusted();

    public Task<TrustStoreBusiness?> GetCa(int id);

    public void RemoveCa(TrustStoreBusiness store);
}