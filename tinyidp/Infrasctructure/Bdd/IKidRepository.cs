
namespace tinyidp.infrastructure.bdd;

public interface IKidRepository
{
    public void Add(Kid certificate);
    public void Remove(Kid certificate);
    public void Update(Kid certificate);
    public Task<List<Kid>> GetAll();
}