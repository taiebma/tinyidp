using tinyidp.Business.BusinessEntities;

namespace tinyidp.Pages.Models;

public class KidView
{
    public int Id { get; set; }

    public string Kid { get; set; } = null!;

    public AlgoType Algo { get; set; }

    public KidState State { get; set; }

    public DateTime CreationDate { get; set; }

    public bool Valid { get; set; }

    public string KeyError { get; set; } = null!;

    public bool CanAccess { get; set; } = false;
}