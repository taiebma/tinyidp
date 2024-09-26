
namespace tinyidp.infrastructure.bdd;

public class BddConfig
{
    public const string BddSection = "bdd";

    public string ServerName { get ; set ;  } = String.Empty;
    public string BddName { get ; set ;  } = String.Empty;
    public string UserName { get ; set ;  } = String.Empty;
    public string Password { get ; set ;  } = String.Empty;
    
}