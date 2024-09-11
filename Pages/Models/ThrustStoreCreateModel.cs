using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using tinyidp.Business.BusinessEntities;

namespace tinyidp.Pages.Models;

public partial class ThrustStoreCreateModel
{
    public string? ExceptionMessage { get; set; } = null;

    public bool CanAccess { get; set; } = false;
}
