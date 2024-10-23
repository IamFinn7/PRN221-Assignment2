using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PizzaOrderWebsite.Pages.PizzaWeb
{
    public class AccountViewModel : PageModel
    {
        private readonly PizzaStore.Data.PizzaContext _context;

        public AccountViewModel(PizzaStore.Data.PizzaContext context)
        {
            _context = context;
        }

        public IList<Account> Account { get; set; } = default!;

        public Account? CurrentAccount { get; set; }

        [BindProperty]
        public Account Accounts { get; set; } = default!;
        public async Task OnGetAsync()
        {
            var accountName = User.Identity.Name;
            var account = await _context.Accounts.SingleOrDefaultAsync(it => it.UserName == accountName);
            bool isStaff = User.IsInRole("Staff");
            bool isMember = User.IsInRole("Member");
            if (isStaff)
            {
                Account = await _context.Accounts.ToListAsync();
            }
            else if (isMember)
            {
                CurrentAccount = await _context.Accounts.SingleOrDefaultAsync(it => it.UserName == accountName);
                if (CurrentAccount == null)
                {
                }
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
