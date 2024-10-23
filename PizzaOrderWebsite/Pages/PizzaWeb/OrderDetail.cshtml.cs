using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PizzaOrderWebsite.Pages.PizzaWeb
{
    public class OrderDetailModel : PageModel
    {
        private readonly PizzaContext _context;

        public OrderDetailModel(PizzaContext context)
        {
            _context = context;
        }

        public Orders Order { get; set; } = default!;

        public async Task OnGetAsync(int? id)
        {
            Order = await _context.Orders.Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.OrderID == id);
        }
    }
}
