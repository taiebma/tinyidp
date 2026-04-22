

using Microsoft.EntityFrameworkCore;

namespace tinyidp.infrastructure.bdd;

public class TokenRepository : ITokenRepository
{
    private readonly IDbContextFactory<TinyidpContext> _tinyidpContext;
    
    public TokenRepository( IDbContextFactory<TinyidpContext> tinyidpContext)
    {
        _tinyidpContext = tinyidpContext;
    }

    public void Add(Token token)
    {
        using var context = _tinyidpContext.CreateDbContext();
        context.Add(token);
        context.SaveChanges();
    }

    public void Remove(Token token)
    {
        using var context = _tinyidpContext.CreateDbContext();
        context.Remove(token);
        context.SaveChanges();
    }

    public void Update(Token token)
    {
        using var context = _tinyidpContext.CreateDbContext();
        context.Update(token);
        context.SaveChanges();
    }
}
