using System;
using System.Collections.Generic;

namespace tinyidp.infrastructure.bdd;

public partial class Kid
{
    public int Id { get; set; }

    public string Kid1 { get; set; } = null!;

    public string Algo { get; set; } = null!;

    public int State { get; set; }

    public string PublicKey { get; set; } = null!;

    public string PrivateKey { get; set; } = null!;

    public DateTime CreationDate { get; set; }
}
