

namespace tinyidp.infrastructure.bdd;

public class TokenRepository : ITokenRepository
{
    private readonly TinyidpContext _tinyidpContext;
    
    public TokenRepository( TinyidpContext tinyidpContext)
    {
        _tinyidpContext = tinyidpContext;
    }

    public void Add(Token token)
    {
        _tinyidpContext.Add(token);
        _tinyidpContext.SaveChanges();
    }

    public void Remove(Token token)
    {
        _tinyidpContext.Remove(token);
        _tinyidpContext.SaveChanges();
    }

    public void Update(Token token)
    {
        _tinyidpContext.Update(token);
        _tinyidpContext.SaveChanges();
    }
}
