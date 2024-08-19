
using System.ComponentModel.DataAnnotations;

namespace tinyidp.Pages.Models;

public class LoginModel
{
    [Required]
    [StringLength(60, MinimumLength = 3)]
    public string Login { get; set; } = null!;

    [Required]
    [RegularExpression(@"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[*.!@$%^&(){}[\]:;<>,.?\/~_+-=|]).{8,32}$")]
    public string Password { get; set; } = null!;

    public string? ExceptionMessage { get; set; } = null;
}