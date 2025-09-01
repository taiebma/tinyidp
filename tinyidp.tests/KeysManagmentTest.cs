using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using tinyidp.Business.BusinessEntities;
using tinyidp.Encryption;
using tinyidp.Exceptions;
using tinyidp.infrastructure.bdd;
using tinyidp.infrastructure.keysmanagment;

namespace tinyidp.tests;

public class KeysManagmentTest
{
    private readonly Mock<IConfiguration> _confMock;
    private readonly Mock<ILogger<KeysManagment>> _loggerMock;
    private readonly Mock<IEncryptionService> _encryptionServiceMock;
    private readonly Mock<IKidRepository> _kidRepositoryMock;
    private readonly IMemoryCache _memoryCache;

    private readonly KeysManagment _keysManagment;

    public KeysManagmentTest()
    {
        List<Kid> kids = new List<Kid> {
            new Kid { Algo = "RSA", CreationDate = DateTime.Now, Id = 1, Kid1 = "Cle1", State = (int)KidState.Inactive, PrivateKey = "", PublicKey = "" },
            new Kid { Algo = "ECC", CreationDate = DateTime.Now.AddDays(-1), Id = 2, Kid1 = "Cle2", State = (int)KidState.Active, PrivateKey = "", PublicKey = "" },
            new Kid { Algo = "ECC", CreationDate = DateTime.Now, Id = 3, Kid1 = "Cle3", State = (int)KidState.Active, PrivateKey = "", PublicKey = "" }
        };
        _confMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<KeysManagment>>();
        _encryptionServiceMock = new Mock<IEncryptionService>();
        _kidRepositoryMock = new Mock<IKidRepository>();
        _memoryCache = new MemoryCache(
            new MemoryCacheOptions
            {
            });

        _encryptionServiceMock.Setup(x => x.Decrypt(It.IsAny<string>())).Returns("-----BEGIN EC PRIVATE KEY-----MHcCAQEEIHhm2oIksyWWa5PViewkMcOKShR2b4SNNmy+vlgvla1hoAoGCCqGSM49AwEHoUQDQgAEntb5t/nxNKO1+AzL/kARU4Mw0cWvPcVFQ5BwuZf+ZmRLwwE88xOIJPY1IID3znqv8PB4Nu9FulmKEYl9rojFTw==-----END EC PRIVATE KEY-----");
        _kidRepositoryMock.Setup(x => x.GetAll()).Returns(Task.FromResult<List<Kid>>(kids));
        _kidRepositoryMock.Setup(x => x.Add(It.IsAny<Kid>()));
        _kidRepositoryMock.Setup(x => x.Update(It.IsAny<Kid>()));
        _kidRepositoryMock.Setup(x => x.Remove(It.IsAny<Kid>()));

        _keysManagment = new KeysManagment(_confMock.Object, _loggerMock.Object, _encryptionServiceMock.Object, _kidRepositoryMock.Object, _memoryCache);
    }

    [Fact]
    public void GetActiveKeys_ReturnActiveKeys()
    {
        List<KidBusinessEntity> kids = _keysManagment.GetActiveKeys();

        Assert.NotEmpty(kids);
        Assert.All(kids, x => Assert.True(x.State == KidState.Active));
    }

    [Fact]
    public void GenNewRSAKey_ReturnOk()
    {
        string kid = "MaCle";

        KidBusinessEntity key = _keysManagment.GenNewRSAKey(kid);

        Assert.Equal(AlgoType.RSA, key.Algo);
        Assert.Equal(KidState.Inactive, key.State);
        Assert.Equal("MaCle", key.Kid1);
        Assert.NotEmpty(key.PrivateKey);        
        Assert.NotEmpty(key.PublicKey);
        _kidRepositoryMock.Verify(x => x.Add(It.IsAny<Kid>()), Times.Once);

    }

    [Fact]
    public void GenNewECCKey_ReturnOk()
    {
        string kid = "MaCle";

        KidBusinessEntity key = _keysManagment.GenNewECCKey(kid);

        Assert.Equal(AlgoType.ECC, key.Algo);
        Assert.Equal(KidState.Inactive, key.State);
        Assert.Equal("MaCle", key.Kid1);
        Assert.NotEmpty(key.PrivateKey);        
        Assert.NotEmpty(key.PublicKey);        
        _kidRepositoryMock.Verify(x => x.Add(It.IsAny<Kid>()), Times.Once);
    }

    [Fact]
    public void GetKeyByKid_ReturnKo()
    {
        string kid = "MaCle";

        KidBusinessEntity? key = _keysManagment.GetKeyByKid(kid);

        Assert.Null(key);
    }

    [Fact]
    public void GetKeyByKid_ReturnOk()
    {
        string kid = "Cle3";

        KidBusinessEntity? key = _keysManagment.GetKeyByKid(kid);

        Assert.NotNull(key);
    }

    [Fact]
    public void GetKeyById_ReturnKo()
    {

        KidBusinessEntity? key = _keysManagment.GetKeyById(5);

        Assert.Null(key);
    }

    [Fact]
    public void GetKeyById_ReturnOk()
    {

        KidBusinessEntity? key = _keysManagment.GetKeyById(1);

        Assert.NotNull(key);
    }

    [Fact]
    public void LastActive_ReturnOk()
    {

        KidBusinessEntity? key = _keysManagment.LastActive(AlgoType.ECC);

        Assert.NotNull(key);
        Assert.Equal("Cle3", key.Kid1);
    }

    [Fact]
    public void LastActive_ReturnKo()
    {

        KidBusinessEntity? key = _keysManagment.LastActive(AlgoType.RSA);

        Assert.Null(key);
    }

    [Fact]
    public void GenerateJWTToken_ValidKey_ReturnOk()
    {
        List<string> scopes =  new List<string> { "scope1", "scope2" };
        List<string> audience = new List<string> { "aud1", "aud2" };
        string sub = "ns1:my-identity";
        long lifeTime = 3600;

        string token = _keysManagment.GenerateJWTToken(AlgoKeyType.ES256, scopes, audience, sub, lifeTime, null);

        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(token);
        Assert.Equal(sub, jwtSecurityToken.Subject);
        Assert.Contains(scopes[0], jwtSecurityToken.Claims.Where(p => p.Type == "scope").First().Value);
        Assert.Contains(scopes[1], jwtSecurityToken.Claims.Where(p => p.Type == "scope").First().Value);
        Assert.Contains(audience[0], jwtSecurityToken.Audiences);
        Assert.Contains(audience[1], jwtSecurityToken.Audiences);
        Assert.NotEmpty(jwtSecurityToken.Issuer);
        Assert.Equal(lifeTime, (jwtSecurityToken.ValidTo - jwtSecurityToken.ValidFrom).TotalMinutes);
    }

    [Fact]
    public void GenerateJWTToken_NoValidKey_ReturnOk()
    {
        List<string> scopes = new List<string> { "scope1", "scope2" };
        List<string> audience = new List<string> { "aud1", "aud2" };
        string sub = "ns1:my-identity";
        long lifeTime = 3600;

        Assert.Throws<TinyidpKeyException>(() =>  _keysManagment.GenerateJWTToken(AlgoKeyType.RS256, scopes, audience, sub, lifeTime, null));

    }
}
