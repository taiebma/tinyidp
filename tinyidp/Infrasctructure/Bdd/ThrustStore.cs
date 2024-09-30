using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tinyidp.infrastructure.bdd;

public partial class ThrustStore
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Dn { get; set; } = null!;

    public string Issuer { get; set; } = null!;

    public DateTime ValidityDate { get; set; }

    public string Certificate { get; set; } = null!;
}
