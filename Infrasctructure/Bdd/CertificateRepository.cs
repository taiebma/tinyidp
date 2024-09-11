

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

    public Task<List<ThrustStore>> GetThrustStore()
    {
        return _tinyidpContext.ThrustStore.AsNoTracking().OrderBy(p => p.Dn).ToListAsync();
    }

    public void AddThrustCertificate(ThrustStore certificate)
    {
        _tinyidpContext.ThrustStore.Add(certificate);
        _tinyidpContext.SaveChanges();
    }

    public void RemoveThrustCertificate(ThrustStore certificate)
    {
        _tinyidpContext.ThrustStore.Remove(certificate);
        _tinyidpContext.SaveChanges();
    }

    public void UpdateThrustCertificate(ThrustStore certificate)
    {
        _tinyidpContext.ThrustStore.Update(certificate);
        _tinyidpContext.SaveChanges();
    }

    public Task<ThrustStore?> GetThrustCertificate(int id)
    {
        return _tinyidpContext.ThrustStore.Where(p => p.Id == id)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

}
