namespace tinyidp.Business.BusinessEntities;

public class KidBusinessEntity
{
    public int Id { get; set; }

    public string Kid1 { get; set; } = null!;

    public AlgoType Algo { get; set; }

    public KidState State { get; set; }

    public string PublicKey { get; set; } = null!;

    public string PrivateKey { get; set; } = null!;

    public DateTime CreationDate { get; set; }

    public bool Valid { get; set; }

    public string KeyError { get; set; } = null!;
}