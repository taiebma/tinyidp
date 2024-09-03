

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
        _tinyidpContext.Add(certificate);
        _tinyidpContext.SaveChanges();
    }

    public void Remove(Certificate certificate)
    {
        _tinyidpContext.Remove(certificate);
        _tinyidpContext.SaveChanges();
    }

    public void Update(Certificate certificate)
    {
        _tinyidpContext.Update(certificate);
        _tinyidpContext.SaveChanges();
    }

    public Task<Certificate?> Get(int id)
    {
        return _tinyidpContext.Certificates.Where(p => p.Id == id)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }
}
