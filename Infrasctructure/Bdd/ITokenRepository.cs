
namespace tinyidp.infrastructure.bdd;

public interface ITokenRepository
{
    public void Add(Token certificate);
    public void Remove(Token certificate);
    public void Update(Token certificate);
}