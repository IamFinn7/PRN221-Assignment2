using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PizzaOrderWebsite.Pages.PizzaWeb
{
    public class AccountCreateModel : PageModel
    {
        private readonly PizzaStore.Data.PizzaContext _context;

        public AccountCreateModel(PizzaStore.Data.PizzaContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Account Account { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            var existUserName = await _context.Accounts.FirstOrDefaultAsync(it => it.UserName == Account.UserName);
            if (existUserName != null)
            {
                ModelState.AddModelError(string.Empty, $"UserName {Account.UserName} already exists.");
                return Page();
            }

            Account accountDto = new Account
            {
                UserName = Account.UserName,
                FullName = Account.FullName,
                Password = Account.Password,
                Type = Account.Type
            };

            _context.Accounts.Add(accountDto);
            await _context.SaveChangesAsync();

            return RedirectToPage("./AccountView");
        }
    }
}
