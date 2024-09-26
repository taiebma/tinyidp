using tinyidp.Business.BusinessEntities;

namespace tinyidp.Pages.Models;

public class KidCreateModel
{
    public string? Kid { get; set; } = null!;

    public AlgoType Algo { get; set; }

    public string? ExceptionMessage { get; set; } = null;

    public bool CanAccess { get; set; } = false;
}