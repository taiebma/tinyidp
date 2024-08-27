
using tinyidp.Business.BusinessEntities;

namespace tinyidp.infrastructure.keysmanagment;

public interface IKeysManagment
{
    public List<KidBusinessEntity> GetKeys();
    public List<KidBusinessEntity> GetActiveKeys();
    public KidBusinessEntity GenNewKey(AlgoType algo, string kid);
    public KidBusinessEntity? GetKeyByKid(string kid);
    public KidBusinessEntity? GetKeyById(int id);
    public KidBusinessEntity? LastActive(AlgoType algo);
    public void Update(KidBusinessEntity kid);
    public void Remove(KidBusinessEntity kid);
    public string GenerateJWTToken(AlgoKeyType keyType, IEnumerable<string> scopes, IEnumerable<string> audience, string? sub, long lifeTime);
}