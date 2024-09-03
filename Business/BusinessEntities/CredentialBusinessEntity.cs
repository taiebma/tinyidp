using tinyidp.infrastructure.bdd;

namespace tinyidp.Business.BusinessEntities;

public partial class CredentialBusinessEntity
{
    public int Id { get; set; }

    public string Ident { get; set; } = null!;

    public string Pass { get; set; } = null!;
    public string PassNew { get; set; } = null!;

    public StateCredential State { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime? LastIdent { get; set; }

    public RoleCredential RoleIdent { get; set; }

    public int NbMaxRenew { get; set; }

    public long TokenMaxMinuteValidity { get; set; }

    public long RefreshMaxMinuteValidity { get; set; }
    
    public bool MustChangePwd { get; set; }

    public IEnumerable<string>? Audiences { get; set; } = null!;

    public IEnumerable<string>? AllowedScopes { get; set; } = null!;

    public string? AuthorizationCode { get; set; } = null!;

    public string? RedirectUri { get; set; } = null!;

    public string? CodeChallenge { get; set; } = null!;
    
    public string? CodeChallengeMethod { get; set; } = null!;
    
    public string? RefreshToken { get; set; } = null!;

    public DateTime? CreationDateRefreshToken { get; set; }

    public AlgoKeyType KeyType { get; set; } 

    public ICollection<CertificateBusinessEntity>? CertificateBusinessEntities { get; set; } = null!;

    public CredentialBusinessEntity()
    {
        CreationDate = DateTime.Now;
    }
}