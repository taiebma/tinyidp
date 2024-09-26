using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace tinyidp.Pages.Models;

public partial class ThrustStoreViewModel
{
    public int Id { get; set; }

    [Required]
    public string Dn { get; set; } = null!;

    [Required]
    public string Issuer { get; set; } = null!;

    public DateTime ValidityDate { get; set; }

}
