
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using tinyidp.Business.BusinessEntities;

namespace tinyidp.Pages.Models;

public class CredentialView
{
    public int Id { get; set; }

    public string Ident { get; set; } = null!;

    public string Pass { get; set; } = null!;

    public StateCredential State { get; set; }

    public RoleCredential RoleIdent { get; set; }

    public int NbMaxRenew { get; set; }

    public long TokenMaxMinuteValidity { get; set; }

    public long RefreshMaxMinuteValidity { get; set; }

    public string? ExceptionMessage { get; set; } = null;

    public DateTime CreationDate { get; set; }

    public DateTime? LastIdent { get; set; }

    public bool CanAccess { get; set; } = false;
}