
using tinyidp.Business.BusinessEntities;

namespace tinyidp.Pages.Models;

 public class ApplicationUser
 {
    public string Ident { get; set; } = null!;
    public int Id { get; set; }

    public RoleCredential Role { get; set; }

    public string? RedirectUrl { get; set; }  
    
 }