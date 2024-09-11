using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace tinyidp.infrastructure.bdd;

public class CredentialRepository : ICredentialRepository
{
    private readonly TinyidpContext _tinyidpContext;
    
    public CredentialRepository( TinyidpContext tinyidpContext)
    {
        _tinyidpContext = tinyidpContext;
    }

    public void Add(Credential credential)
    {
        _tinyidpContext.Credentials.Add(credential);
        _tinyidpContext.SaveChanges();
    }

    public void Remove(Credential credential)
    {
        _tinyidpContext.Credentials.Remove(credential);
        _tinyidpContext.SaveChanges();
    }

    public void Update(Credential credential)
    {
        _tinyidpContext.Credentials.Update(credential);
    }

    public Credential? GetById(int id)
    {
        return _tinyidpContext.Credentials
            .Where<Credential>(p => p.Id == id)
            .FirstOrDefault();
    }

    public Task<Credential?> GetByIdent(string ident)
    {
        return _tinyidpContext.Credentials
            .Where<Credential>(p => p.Ident == ident).FirstOrDefaultAsync();
    }

    public Credential? GetByIdReadOnly(int id)
    {
        return _tinyidpContext.Credentials
            .Where<Credential>(p => p.Id == id).AsNoTracking()
            .FirstOrDefault();
    }

    public Credential? GetWithCertificates(int id)
    {
        return _tinyidpContext.Credentials
            .Where<Credential>(p => p.Id == id)
            .AsNoTracking()
            .Include(p => p.Certificates)
            .FirstOrDefault();
    }

    public Task<Credential?> GetByIdentReadOnly(string ident)
    {
        return _tinyidpContext.Credentials
            .Where<Credential>(p => p.Ident == ident).AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public Task<List<Credential>> SearchByIdentLike(string ident)
    {
        return _tinyidpContext.Credentials
            .Where<Credential>(p => p.Ident.Contains(ident))
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync();
    }

    public Task<Credential?> GetByAuthorizationCode(string code)
    {
        return _tinyidpContext.Credentials
            .Where<Credential>(p => p.AuthorizationCode == code)
            .AsNoTracking().FirstOrDefaultAsync();
    }

    public Task<Credential?> GetByRefreshToken(string token)
    {
        return _tinyidpContext.Credentials
            .Where<Credential>(p => p.RefreshToken == token)
            .AsNoTracking().FirstOrDefaultAsync();
    }

    public Task<List<Credential>> SearchByState(int state )
    {
        return _tinyidpContext.Credentials
            .Where<Credential>(p => p.State == state)
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync();
    }

    public Task<List<Credential>> GetAll( )
    {
        return _tinyidpContext.Credentials.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
    }

    public Task<Credential?> GetCredentialByCertificate(string serial, string issuer)
    {
        return _tinyidpContext.Credentials.Where(
            p => p.Certificates.Any(c => c.Issuer == issuer && c.Serial == serial))
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public void SaveChanges()
    {
        _tinyidpContext.SaveChanges();
    }
}
