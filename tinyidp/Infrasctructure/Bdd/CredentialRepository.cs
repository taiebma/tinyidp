using System.ComponentModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace tinyidp.infrastructure.bdd;

public class CredentialRepository : ICredentialRepository
{
    private readonly TinyidpContext _tinyidpContext;
    private readonly ICacheTinyidp<Credential> _cache;
    private readonly BackgroundSaveDB _backgroundSaveDB;

    private static readonly Func<TinyidpContext, string, Task<Credential?>> _getByIdent =
        EF.CompileAsyncQuery((TinyidpContext context, string ident) =>
            context.Credentials.Where(u => u.Ident == ident).FirstOrDefault());
        private static readonly Func<TinyidpContext, string, Task<Credential?>> _getByIdentReadonly =
            EF.CompileAsyncQuery((TinyidpContext context, string ident) =>
            context.Credentials.Where(u => u.Ident == ident).AsNoTracking().FirstOrDefault());
        private static readonly Func<TinyidpContext, string, Task<Credential?>> _getByAuthorizationCode =
            EF.CompileAsyncQuery((TinyidpContext context, string code) =>
            context.Credentials.Where(u => u.AuthorizationCode == code).AsNoTracking().FirstOrDefault());
        private static readonly Func<TinyidpContext, string, Task<Credential?>> _getByRefreshToken =
            EF.CompileAsyncQuery((TinyidpContext context,  string token) =>
            context.Credentials.Where(u => u.RefreshToken == token).AsNoTracking().FirstOrDefault());

    public CredentialRepository( TinyidpContext tinyidpContext, ICacheTinyidp<Credential> cache, BackgroundSaveDB backgroundSaveDB)
    {
        _tinyidpContext = tinyidpContext;
        _cache = cache;
        _backgroundSaveDB = backgroundSaveDB;
    }

    public void Add(Credential credential)
    {
        _cache.Set(credential.Ident, credential);
        _tinyidpContext.Credentials.Add(credential);
        _tinyidpContext.SaveChanges();
    }

    public void Remove(Credential credential)
    {
        _cache.Remove(credential.Ident);
        _tinyidpContext.Credentials.Remove(credential);
        _tinyidpContext.SaveChanges();
    }

    public void Update(Credential credential)
    {
        _cache.Set(credential.Ident, credential);
        _tinyidpContext.Credentials.Update(credential);
        _tinyidpContext.SaveChanges();
    }

    public async Task<Credential?> GetById(int id)
    {
        return await _tinyidpContext.Credentials
            .Where<Credential>(p => p.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<Credential?> GetByIdent(string ident)
    {
        var cached = _cache.Get(ident);
        if (cached != null)
        {
            _tinyidpContext.Credentials.Attach(cached);
            return cached;
        }
        return await _getByIdent(_tinyidpContext, ident);
    }

    public async Task<Credential?> GetByIdReadOnly(int id)
    {
        return await _tinyidpContext.Credentials
            .Where<Credential>(p => p.Id == id).AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<Credential?> GetWithCertificates(int id)
    {
        return await _tinyidpContext.Credentials
            .Where<Credential>(p => p.Id == id)
            .AsNoTracking()
            .Include(p => p.Certificates)
            .FirstOrDefaultAsync();
    }

    public async Task<Credential?> GetByIdentReadOnly(string ident)
    {
        return await _getByIdentReadonly(_tinyidpContext, ident);
    }

    public async Task<List<Credential>> SearchByIdentLike(string ident)
    {
        return await _tinyidpContext.Credentials
            .Where<Credential>(p => p.Ident.Contains(ident))
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync();
    }

    public async Task<Credential?> GetByAuthorizationCode(string code)
    {
        return await _getByAuthorizationCode(_tinyidpContext, code);
    }

    public async Task<Credential?> GetByRefreshToken(string token)
    {
        return await _getByRefreshToken(_tinyidpContext, token);
    }

    public async Task<List<Credential>> SearchByState(int state )
    {
        return await _tinyidpContext.Credentials
            .Where<Credential>(p => p.State == state)
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync();
    }

    public async Task<List<Credential>> GetAll( )
    {
        return await _tinyidpContext.Credentials.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
    }

    public async Task<Credential?> GetCredentialByCertificate(string serial, string issuer)
    {
        return await _tinyidpContext.Credentials.Where(
            p => p.Certificates.Any(c => c.Issuer == issuer && c.Serial == serial))
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveChanges()
    {
        return await _tinyidpContext.SaveChangesAsync();
    }

    public void DeferredSaveChanges()
    {
        _tinyidpContext.ChangeTracker.Entries<Credential>()
            .Where(e => e.State == EntityState.Modified)
            .ToList()
            .ForEach(e => {
                var cred = e.Entity.Clone();
                _cache.Set(e.Entity.Ident, cred);
                _backgroundSaveDB.EnqueueAsync(cred).GetAwaiter().GetResult();
                e.State = EntityState.Unchanged; // Empêche EF de faire le update immédiatement
            });
    }
}
