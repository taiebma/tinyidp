
using tinyidp.Business.BusinessEntities;

namespace tinyidp.Business.keysmanagment;

public interface IKeysManagment
{
    public Task<List<KidBusinessEntity>> GetKeys();
    public Task<List<KidBusinessEntity>> GetActiveKeys();
    public Task<KidBusinessEntity> GenNewKey(AlgoType algo, string kid);
    public Task<KidBusinessEntity?> GetKeyByKid(string kid);
    public Task<KidBusinessEntity?> GetKeyById(int id);
    public Task<KidBusinessEntity?> LastActive(AlgoType algo);
    public Task Update(KidBusinessEntity kid);
    public Task Remove(KidBusinessEntity kid);
    public Task<string> GenerateJWTToken(AlgoKeyType keyType, IEnumerable<string> scopes, IEnumerable<string> audience, string? sub, long lifeTime, string ?nonce);
}