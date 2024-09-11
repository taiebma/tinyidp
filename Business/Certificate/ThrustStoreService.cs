using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using tinyidp.Business.BusinessEntities;
using tinyidp.Exceptions;
using tinyidp.Extensions;
using tinyidp.infrastructure.bdd;

namespace tinyidp.Business.Certificate;

public class ThrustStoreService : IThrustStoreService
{
    private readonly ILogger<ThrustStoreService> _logger;
    private readonly ICertificateRepository _certificateRepository;
    private readonly IMemoryCache _thrustedStore;

    public ThrustStoreService(ILogger<ThrustStoreService> logger, ICertificateRepository certificateRepository, IMemoryCache memoryCache)
    {
        _logger = logger;
        _certificateRepository = certificateRepository;
        _thrustedStore = memoryCache;
    }

    public async Task<bool> VerifyWithChain(X509Certificate2 cert)
    {
        _thrustedStore.TryGetValue("thrustedStore", out List<ThrustStore>? listThrustedCertificate);
        if (listThrustedCertificate == null)
        {
            listThrustedCertificate = await _certificateRepository.GetThrustStore();
            _thrustedStore.Set("thrustedStore", listThrustedCertificate );
        }

        using X509Chain chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        foreach(ThrustStore ca in listThrustedCertificate)
        {
            X509Certificate2 x509 = X509Certificate2.CreateFromPem(ca.Certificate.AsSpan());
            chain.ChainPolicy.ExtraStore.Add(x509);
        }
        chain.Build(cert);

        if (chain.ChainStatus.Length > 0)
        {
            throw new TinyidpCertificateException(chain.ChainStatus[0].StatusInformation);
        }
        return (chain.ChainStatus.Length == 0);
    }

    public void AddCaToStore(string dn, string issuer, DateTime validityDate, X509Certificate2 certificate)
    {
        ThrustStore ca = new ThrustStore() {
            Certificate = certificate.ExportCertificatePem(), Dn = dn, Issuer = issuer, ValidityDate = validityDate
        };
        _certificateRepository.AddThrustCertificate(ca);
        _thrustedStore.Remove("thrustedStore");
    }

    public async Task<List<ThrustStoreBusiness>> GetAllCaThrusted()
    {
        List<ThrustStore> store = await _certificateRepository.GetThrustStore();
        return store.Select(p => p.ToBusiness()).ToList();
    }

    public async Task<ThrustStoreBusiness?> GetCa(int id)
    {
        ThrustStore? ca = await _certificateRepository.GetThrustCertificate(id);
        return ca?.ToBusiness();
    }

    public void RemoveCa(ThrustStoreBusiness store)
    {
        _certificateRepository.RemoveThrustCertificate(store.ToEntity());
    }
}