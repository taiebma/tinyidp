namespace tinyidp.Business.BusinessEntities;


public partial class ThrustStoreBusiness
{
    public int Id { get; set; }

    public string Dn { get; set; } = null!;

    public string Issuer { get; set; } = null!;

    public DateTime ValidityDate { get; set; }

    public string Certificate { get; set; } = null!;
}