
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using tinyidp.Business.BusinessEntities;

namespace tinyidp.Pages.Models;

public class ChangePwdModel
{
    public int Id { get; set; }

    public string Ident { get; set; } = null!;

    [Required]
    public string Pass { get; set; } = null!;

    [RegularExpression(@"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[*.!@$%^&(){}[\]:;<>,.?\/~_+-=|]).{8,32}$")]
    [Required]
    public string PassNew { get; set; } = null!;

    public string? ExceptionMessage { get; set; } = null;

}