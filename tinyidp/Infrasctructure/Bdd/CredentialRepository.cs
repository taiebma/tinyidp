using System.ComponentModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace tinyidp.infrastructure.bdd;

public class CredentialRepository : ICredentialRepository
{
    private readonly IDbContextFactory<TinyidpContext> _tinyidpContext;
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

    public CredentialRepository( IDbContextFactory<TinyidpContext> tinyidpContext, ICacheTinyidp<Credential> cache, BackgroundSaveDB backgroundSaveDB)
    {
        _tinyidpContext = tinyidpContext;
        _cache = cache;
        _backgroundSaveDB = backgroundSaveDB;
    }

    public void Add(Credential credential)
    {
        _cache.Set(credential.Ident, credential);
        using var context = _tinyidpContext.CreateDbContext();
        context.Credentials.Add(credential);
        context.SaveChanges();
    }

    public void Remove(Credential credential)
    {
        _cache.Remove(credential.Ident);
        using var context = _tinyidpContext.CreateDbContext();
        context.Credentials.Remove(credential);
        context.SaveChanges();
    }

    public void Update(Credential credential)
    {
        _cache.Set(credential.Ident, credential);
        using var context = _tinyidpContext.CreateDbContext();
        context.Credentials.Update(credential);
        context.SaveChanges();
    }

    public Credential? GetById(int id)
    {
        using var context = _tinyidpContext.CreateDbContext();
        return context.Credentials
            .Where<Credential>(p => p.Id == id)
            .FirstOrDefault();
    }

    public Credential? GetByIdent(string ident)
    {
        using var context = _tinyidpContext.CreateDbContext();
        var cached = _cache.Get(ident);
        if (cached != null)
        {
            context.Credentials.Attach(cached);
            return cached;
        }
        return _getByIdent(context, ident).Result;
    }

    public Credential? GetByIdReadOnly(int id)
    {
        using var context = _tinyidpContext.CreateDbContext();
        return context.Credentials
            .Where<Credential>(p => p.Id == id).AsNoTracking()
            .FirstOrDefault();
    }

    public Credential? GetWithCertificates(int id)
    {
        using var context = _tinyidpContext.CreateDbContext();
        return context.Credentials
            .Where<Credential>(p => p.Id == id)
            .AsNoTracking()
            .Include(p => p.Certificates)
            .FirstOrDefault();
    }

    public Task<Credential?> GetByIdentReadOnly(string ident)
    {
        using var context = _tinyidpContext.CreateDbContext();
        return _getByIdentReadonly(context, ident);
    }

    public Task<List<Credential>> SearchByIdentLike(string ident)
    {
        using var context = _tinyidpContext.CreateDbContext();
        return context.Credentials
            .Where<Credential>(p => p.Ident.Contains(ident))
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync();
    }

    public Task<Credential?> GetByAuthorizationCode(string code)
    {
        using var context = _tinyidpContext.CreateDbContext();
        return _getByAuthorizationCode(context, code);
    }

    public Task<Credential?> GetByRefreshToken(string token)
    {
        using var context = _tinyidpContext.CreateDbContext();
        return _getByRefreshToken(context, token);
    }

    public Task<List<Credential>> SearchByState(int state )
    {
        using var context = _tinyidpContext.CreateDbContext();
        return context.Credentials
            .Where<Credential>(p => p.State == state)
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync();
    }

    public async Task<List<Credential>> GetAll( )
    {
        using var context = _tinyidpContext.CreateDbContext();
        return await context.Credentials.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
    }

    public Task<Credential?> GetCredentialByCertificate(string serial, string issuer)
    {
        using var context = _tinyidpContext.CreateDbContext();
        return context.Credentials.Where(
            p => p.Certificates.Any(c => c.Issuer == issuer && c.Serial == serial))
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveChanges()
    {
        using var context = _tinyidpContext.CreateDbContext();
        return await context.SaveChangesAsync();
    }

    public void DeferredSaveChanges()
    {
        using var context = _tinyidpContext.CreateDbContext();
        context.ChangeTracker.Entries<Credential>()
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
