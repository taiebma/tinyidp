using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace tinyidp.Pages.Account
{
    public class SignedOutModel : PageModel
    {
        public async Task<IActionResult> OnPostAsync()
        {
            if (User.Identity?.IsAuthenticated??false)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToPage("/Index");
            }

            return Page();
        }
        public async Task<IActionResult> OnGetAsync()
        {
            if (User.Identity?.IsAuthenticated??false)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToPage("/Index");
            }

            return Page();
        }
    }
}