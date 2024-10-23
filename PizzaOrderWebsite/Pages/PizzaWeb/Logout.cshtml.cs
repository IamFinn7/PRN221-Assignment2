using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PizzaOrderWebsite.Pages.PizzaWeb
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            HttpContext.Session.Remove("CartItemCount");
            await HttpContext.SignOutAsync();
            return RedirectToPage("/Index");
        }
    }
}
