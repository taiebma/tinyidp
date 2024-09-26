using System;
using System.Collections.Generic;

namespace tinyidp.infrastructure.bdd;

public partial class Token
{
    public int Id { get; set; }

    public string RefreshToken { get; set; } = null!;

    public int Type { get; set; }

    public int IdCred { get; set; }

    public int NbRenew { get; set; }

    public DateTime? LastRenew { get; set; }
}
