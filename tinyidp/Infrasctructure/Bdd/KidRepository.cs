

using Microsoft.EntityFrameworkCore;

namespace tinyidp.infrastructure.bdd;

public class KidRepository : IKidRepository
{
    private readonly IDbContextFactory<TinyidpContext> _tinyidpContext;
    
    public KidRepository( IDbContextFactory<TinyidpContext> tinyidpContext)
    {
        _tinyidpContext = tinyidpContext;
    }

    public void Add(Kid kid)
    {
        using var context = _tinyidpContext.CreateDbContext();
        context.Add(kid);
        context.SaveChanges();
    }

    public void Remove(Kid kid)
    {
        using var context = _tinyidpContext.CreateDbContext();
        context.Remove(kid);
        context.SaveChanges();
    }

    public void Update(Kid kid)
    {
        using var context = _tinyidpContext.CreateDbContext();
        context.Update(kid);
        context.SaveChanges();
    }

    public Task<List<Kid>> GetAll( )
    {
        using var context = _tinyidpContext.CreateDbContext();
        return context.Kids.OrderBy(p => p.CreationDate).AsNoTracking().ToListAsync();
    }

}
