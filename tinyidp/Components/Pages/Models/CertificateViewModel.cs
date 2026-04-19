using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace tinyidp.Pages.Models;

public partial class CertificateViewModel
{
    public int Id { get; set; }

    [Required]
    public string Dn { get; set; } = null!;

    [Required]
    public string Issuer { get; set; } = null!;

    [Required]
    public string Serial { get; set; } = null!;

    [Required]
    public int State { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime? LastIdent { get; set; }

    [Required]
    public int IdClient { get; set; }

}
