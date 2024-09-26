using System;
using System.Collections.Generic;

namespace tinyidp.infrastructure.bdd;

public partial class Credential
{
    public int Id { get; set; }

    public string Ident { get; set; } = null!;

    public string Pass { get; set; } = null!;

    public int State { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime? LastIdent { get; set; }

    public int RoleIdent { get; set; }

    public int NbMaxRenew { get; set; }

    public long TokenMaxMinuteValidity { get; set; }

    public long RefreshMaxMinuteValidity { get; set; }

    public bool MustChangePwd { get; set; }

    public string? Audiences { get; set; } = null!;

    public string? AllowedScopes { get; set; } = null!;

    public string? AuthorizationCode { get; set; } = null!;

    public string? RedirectUri { get; set; } = null!;

    public string? CodeChallenge { get; set; } = null!;

    public string? CodeChallengeMethod { get; set; } = null!;

    public string? RefreshToken { get; set; } = null!;

    public DateTime? CreationDateRefreshToken { get; set; }

    public int KeyType { get; set; }

    public ICollection<Certificate> Certificates{ get; set; } = null!;
}
