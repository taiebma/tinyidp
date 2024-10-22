using Microsoft.AspNetCore.Identity;

namespace tinyidp.Business.BusinessEntities;

public class AppUser : IdentityUser
{
    public string sub { get; set;} = null!;
    public string name { get; set;} = null!;
}
