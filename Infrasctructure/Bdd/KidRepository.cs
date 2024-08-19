

using Microsoft.EntityFrameworkCore;

namespace tinyidp.infrastructure.bdd;

public class KidRepository : IKidRepository
{
    private readonly TinyidpContext _tinyidpContext;
    
    public KidRepository( TinyidpContext tinyidpContext)
    {
        _tinyidpContext = tinyidpContext;
    }

    public void Add(Kid kid)
    {
        _tinyidpContext.Add(kid);
        _tinyidpContext.SaveChanges();
    }

    public void Remove(Kid kid)
    {
        _tinyidpContext.Remove(kid);
        _tinyidpContext.SaveChanges();
    }

    public void Update(Kid kid)
    {
        _tinyidpContext.Update(kid);
        _tinyidpContext.SaveChanges();
    }

    public Task<List<Kid>> GetAll( )
    {
        return _tinyidpContext.Kids.OrderBy(p => p.CreationDate).AsNoTracking().ToListAsync();
    }

}
