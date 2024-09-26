using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using tinyidp.Business.BusinessEntities;

namespace tinyidp.Pages.Models;

public partial class ThrustStoreEditModel
{
    public int Id { get; set; }

    [Required]
    public string Dn { get; set; } = null!;

    [Required]
    public string Issuer { get; set; } = null!;

    public DateTime ValidityDate { get; set; }

    public string Certificate { get; set; } = null!;

    public string? ExceptionMessage { get; set; } = null;

    public bool CanAccess { get; set; } = false;
}
