

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
        _tinyidpContext.Kids.Add(kid);
        _tinyidpContext.SaveChanges();
    }

    public void Remove(Kid kid)
    {
        _tinyidpContext.Kids.Remove(kid);
        _tinyidpContext.SaveChanges();
    }

    public void Update(Kid kid)
    {
        _tinyidpContext.Kids.Update(kid);
        _tinyidpContext.SaveChanges();
    }

    public async Task<List<Kid>> GetAll( )
    {
        return await _tinyidpContext.Kids.OrderBy(p => p.CreationDate).AsNoTracking().ToListAsync();
    }

}
