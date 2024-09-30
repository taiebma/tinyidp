using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tinyidp.infrastructure.bdd;

public partial class Certificate
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Dn { get; set; } = null!;

    public string Issuer { get; set; } = null!;

    public string Serial { get; set; } = null!;

    public int State { get; set; }

    public DateTime ValidityDate { get; set; }

    public DateTime? LastIdent { get; set; }

    public int IdClient { get; set; }

    public Credential ClientCredential { get; set; } = null!;
}
