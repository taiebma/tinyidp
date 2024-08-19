using System;
using System.Collections.Generic;

namespace tinyidp.infrastructure.bdd;

public partial class Certificate
{
    public int Id { get; set; }

    public string Dn { get; set; } = null!;

    public string Issuer { get; set; } = null!;

    public string Serial { get; set; } = null!;

    public int State { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime? LastIdent { get; set; }

    public int NbMaxRenew { get; set; }

    public long TokenMaxMinuteValidity { get; set; }

    public long RefreshMaxMinuteValidity { get; set; }

    public string Audiences { get; set; } = null!;

    public string AllowedScopes { get; set; } = null!;

    public string? AuthorizationCode { get; set; } = null!;

    public string? RedirectUri { get; set; } = null!;

    public string? CodeChallenge { get; set; } = null!;

    public string? CodeChallengeMethod { get; set; } = null!;
}
