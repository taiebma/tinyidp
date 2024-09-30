using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tinyidp.infrastructure.bdd;

public partial class Token
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string RefreshToken { get; set; } = null!;

    public int Type { get; set; }

    public int IdCred { get; set; }

    public int NbRenew { get; set; }

    public DateTime? LastRenew { get; set; }
}
