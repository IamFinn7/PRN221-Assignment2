using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PizzaOrderWebsite.Pages.PizzaWeb
{
    public class AccountEditModel : PageModel
    {
        private readonly PizzaStore.Data.PizzaContext _context;

        public AccountEditModel(PizzaStore.Data.PizzaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Account Account { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(m => m.AccountID == id);
            if (account == null)
            {
                return NotFound();
            }
            Account = account;
            return Page();
        }
        [Authorize]
        public async Task<IActionResult> OnPostAsync()
        {
            // Load the existing account based on the AccountID
            var accountToUpdate = await _context.Accounts.FindAsync(Account.AccountID);

            if (accountToUpdate == null)
            {
                return NotFound();
            }

            // Update properties
            accountToUpdate.UserName = Account.UserName;
            accountToUpdate.Password = Account.Password; // Remember to hash passwords
            accountToUpdate.FullName = Account.FullName;

            // Set account type based on the logic you have
            var adminAccount = await _context.Accounts.FirstOrDefaultAsync(it => it.Type == AccountType.Staff);
            var isStaff = User.IsInRole("Staff");

            if (adminAccount != null && adminAccount.AccountID == accountToUpdate.AccountID && isStaff)
            {
                accountToUpdate.Type = AccountType.Staff;
            }
            else if (accountToUpdate.Type == AccountType.Member)
            {
                accountToUpdate.Type = AccountType.Member;
            }
            else
            {
                accountToUpdate.Type = null;
            }

            try
            {
                // Save changes to the context
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccountExists(accountToUpdate.AccountID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./AccountView");
        }


        private bool AccountExists(int id)
        {
            return _context.Accounts.Any(e => e.AccountID == id);
        }
    }
}
