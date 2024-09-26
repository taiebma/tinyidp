using System;
using System.Collections.Generic;

namespace tinyidp.infrastructure.bdd;

public partial class ThrustStore
{
    public int Id { get; set; }

    public string Dn { get; set; } = null!;

    public string Issuer { get; set; } = null!;

    public DateTime ValidityDate { get; set; }

    public string Certificate { get; set; } = null!;
}
