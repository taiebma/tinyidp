
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using tinyidp.Business.BusinessEntities;

namespace tinyidp.Pages.Models;

public class CredentialCreateModel
{

    [StringLength(60, MinimumLength = 3)]
    [Required]
    public string Ident { get; set; } = null!;

    [RegularExpression(@"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[*.!@$%^&(){}[\]:;<>,.?\/~_+-=|]).{8,32}$")]
    [Required]
    public string Pass { get; set; } = null!;

    public StateCredential State { get; set; }

    public RoleCredential RoleIdent { get; set; }

    [Column(TypeName = "long")]
    public int NbMaxRenew { get; set; }

    [Column(TypeName = "long")]
    public long TokenMaxMinuteValidity { get; set; }

    [Column(TypeName = "long")]
    public long RefreshMaxMinuteValidity { get; set; }

    public string? Audiences { get; set; } = null!;

    public string? AllowedScopes { get; set; } = null!;

    public string? AuthorizationCode { get; set; } = null!;

    public string? RedirectUri { get; set; } = null!;

    public string? ExceptionMessage { get; set; } = null;

    public bool CanAccess { get; set; } = false;

    public AlgoKeyType? KeyType { get; set; }
}