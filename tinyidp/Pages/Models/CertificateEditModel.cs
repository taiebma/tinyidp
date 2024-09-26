using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using tinyidp.Business.BusinessEntities;

namespace tinyidp.Pages.Models;

public partial class CertificateEditModel
{
    public int Id { get; set; }

    [Required]
    public string Dn { get; set; } = null!;

    [Required]
    public string Issuer { get; set; } = null!;

    [Required]
    public string Serial { get; set; } = null!;

    [Required]
    public StateCredential State { get; set; }

    public DateTime ValidityDate { get; set; }

    public DateTime? LastIdent { get; set; }

    [Required]
    public int IdClient { get; set; }

    public string? ExceptionMessage { get; set; } = null;

    public bool CanAccess { get; set; } = false;
}
