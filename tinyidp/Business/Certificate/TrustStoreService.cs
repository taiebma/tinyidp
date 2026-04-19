using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using tinyidp.Business.BusinessEntities;
using tinyidp.Exceptions;
using tinyidp.Extensions;
using tinyidp.infrastructure.bdd;

namespace tinyidp.Business.Certificate;

public class TrustStoreService : ITrustStoreService
{
    private readonly ILogger<TrustStoreService> _logger;
    private readonly ICertificateRepository _certificateRepository;
    private readonly IMemoryCache _trustedStore;

    public TrustStoreService(ILogger<TrustStoreService> logger, ICertificateRepository certificateRepository, IMemoryCache memoryCache)
    {
        _logger = logger;
        _certificateRepository = certificateRepository;
        _trustedStore = memoryCache;
    }

    public async Task<bool> VerifyWithChain(X509Certificate2 cert)
    {
        _trustedStore.TryGetValue("trustedStore", out List<TrustStore>? listTrustedCertificate);
        if (listTrustedCertificate == null)
        {
            listTrustedCertificate = await _certificateRepository.GetTrustStore();
            _trustedStore.Set("trustedStore", listTrustedCertificate );
        }

        using X509Chain chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.Build(cert);

        if (chain.ChainStatus.Length > 0)
        {
            // Try with custom store
            chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
            foreach(TrustStore ca in listTrustedCertificate)
            {
                X509Certificate2 x509 = X509Certificate2.CreateFromPem(ca.Certificate.AsSpan());
                chain.ChainPolicy.CustomTrustStore.Add(x509);
            }
            chain.Build(cert);
            
            if (chain.ChainStatus.Length > 0)
            {
                throw new TinyidpCertificateException(chain.ChainStatus[0].StatusInformation);
            }
        }
        return (chain.ChainStatus.Length == 0);
    }

    public void AddCaToStore(string dn, string issuer, DateTime validityDate, X509Certificate2 certificate)
    {
        TrustStore ca = new TrustStore() {
            Certificate = certificate.ExportCertificatePem(), Dn = dn, Issuer = issuer, ValidityDate = validityDate
        };
        _certificateRepository.AddTrustCertificate(ca);
        _trustedStore.Remove("trustedStore");
    }

    public async Task<List<TrustStoreBusiness>> GetAllCaTrusted()
    {
        List<TrustStore> store = await _certificateRepository.GetTrustStore();
        return store.Select(p => p.ToBusiness()).ToList();
    }

    public async Task<TrustStoreBusiness?> GetCa(int id)
    {
        TrustStore? ca = await _certificateRepository.GetTrustCertificate(id);
        return ca?.ToBusiness();
    }

    public void RemoveCa(TrustStoreBusiness store)
    {
        _certificateRepository.RemoveTrustCertificate(store.ToEntity());
    }
}