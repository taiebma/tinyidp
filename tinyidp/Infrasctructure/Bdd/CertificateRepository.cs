

using Microsoft.EntityFrameworkCore;

namespace tinyidp.infrastructure.bdd;

public class CertificateRepository : ICertificateRepository
{
    private readonly TinyidpContext _tinyidpContext;
    
    public CertificateRepository( TinyidpContext tinyidpContext)
    {
        _tinyidpContext = tinyidpContext;
    }

    public void Add(Certificate certificate)
    {
        _tinyidpContext.Certificates.Add(certificate);
        _tinyidpContext.SaveChanges();
    }

    public void Remove(Certificate certificate)
    {
        _tinyidpContext.Certificates.Remove(certificate);
        _tinyidpContext.SaveChanges();
    }

    public void Update(Certificate certificate)
    {
        _tinyidpContext.Certificates.Update(certificate);
        _tinyidpContext.SaveChanges();
    }

    public Task<Certificate?> Get(int id)
    {
        return _tinyidpContext.Certificates.Where(p => p.Id == id)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public Task<List<TrustStore>> GetTrustStore()
    {
        return _tinyidpContext.TrustStore.AsNoTracking().OrderBy(p => p.Dn).ToListAsync();
    }

    public void AddTrustCertificate(TrustStore certificate)
    {
        _tinyidpContext.TrustStore.Add(certificate);
        _tinyidpContext.SaveChanges();
    }

    public void RemoveTrustCertificate(TrustStore certificate)
    {
        _tinyidpContext.TrustStore.Remove(certificate);
        _tinyidpContext.SaveChanges();
    }

    public void UpdateTrustCertificate(TrustStore certificate)
    {
        _tinyidpContext.TrustStore.Update(certificate);
        _tinyidpContext.SaveChanges();
    }

    public Task<TrustStore?> GetTrustCertificate(int id)
    {
        return _tinyidpContext.TrustStore.Where(p => p.Id == id)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

}
