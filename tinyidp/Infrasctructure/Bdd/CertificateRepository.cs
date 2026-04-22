

using Microsoft.EntityFrameworkCore;

namespace tinyidp.infrastructure.bdd;

public class CertificateRepository : ICertificateRepository
{
    private readonly IDbContextFactory<TinyidpContext> _tinyidpContext;
    
    public CertificateRepository( IDbContextFactory<TinyidpContext> tinyidpContext)
    {
        _tinyidpContext = tinyidpContext;
    }

    public void Add(Certificate certificate)
    {
        using var context = _tinyidpContext.CreateDbContext();
        context.Certificates.Add(certificate);
        context.SaveChanges();
    }

    public void Remove(Certificate certificate)
    {
        using var context = _tinyidpContext.CreateDbContext();
        context.Certificates.Remove(certificate);
        context.SaveChanges();
    }

    public void Update(Certificate certificate)
    {
        using var context = _tinyidpContext.CreateDbContext();
        context.Certificates.Update(certificate);
        context.SaveChanges();
    }

    public Task<Certificate?> Get(int id)
    {
        using var context = _tinyidpContext.CreateDbContext();
        return context.Certificates.Where(p => p.Id == id)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public Task<List<TrustStore>> GetTrustStore()
    {
        using var context = _tinyidpContext.CreateDbContext();
        return context.TrustStore.AsNoTracking().OrderBy(p => p.Dn).ToListAsync();
    }

    public void AddTrustCertificate(TrustStore certificate)
    {
        using var context = _tinyidpContext.CreateDbContext();
        context.TrustStore.Add(certificate);
        context.SaveChanges();
    }

    public void RemoveTrustCertificate(TrustStore certificate)
    {
        using var context = _tinyidpContext.CreateDbContext();
        context.TrustStore.Remove(certificate);
        context.SaveChanges();
    }

    public void UpdateTrustCertificate(TrustStore certificate)
    {
        using var context = _tinyidpContext.CreateDbContext();
        context.TrustStore.Update(certificate);
        context.SaveChanges();
    }

    public Task<TrustStore?> GetTrustCertificate(int id)
    {
        using var context = _tinyidpContext.CreateDbContext();
        return context.TrustStore.Where(p => p.Id == id)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

}
