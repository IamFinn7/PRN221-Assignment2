using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PizzaStore.Models;

namespace PizzaOrderWebsite.Pages.PizzaWeb
{
    public class MenuModel : PageModel
    {
        private readonly PizzaContext _context;
        private readonly IMapper _mapper;

        public MenuModel(PizzaContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IList<ProductVM> Products { get; set; } = default!;
        public IList<Categories> Categories { get; set; } = default!;
        public IList<Suppliers> Suppliers { get; set; } = default!;

        [BindProperty]
        public ProductVM SelectedProduct { get; set; } = new ProductVM();

        public async Task OnGetAsync(string? searchTerm)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.ProductName.Contains(searchTerm));
            }

            var results = await query.ToListAsync();
            Products = _mapper.Map<List<ProductVM>>(results);

            //Categories = await _context.Categories.ToListAsync();
            //Suppliers = await _context.Suppliers.ToListAsync();
        }

        //public async Task<IActionResult> OnGetEditAsync(int productId)
        //{
        //    try
        //    {
        //        var product = await _context.Products
        //            .Include(p => p.Category)
        //            .Include(p => p.Supplier)
        //            .FirstOrDefaultAsync(m => m.ProductID == productId);

        //        if (product == null)
        //        {
        //            return NotFound();
        //        }

        //        var productVm = _mapper.Map<ProductVM>(product);
        //        productVm.CategoryID = product.Category.CategoryID;
        //        productVm.SupplierID = product.Supplier.SupplierID;

        //        return new JsonResult(productVm);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the error (optional)
        //        return BadRequest(new { error = ex.Message });
        //    }
        //}


        //public async Task<IActionResult> OnPostAsync()
        //{
        //    var productToUpdate = await _context.Products.FindAsync(SelectedProduct.ProductID);
        //    if (productToUpdate == null)
        //    {
        //        return NotFound();
        //    }

        //    productToUpdate.ProductName = SelectedProduct.ProductName;
        //    productToUpdate.UnitPrice = SelectedProduct.UnitPrice;
        //    productToUpdate.ProductImage = SelectedProduct.ProductImage;
        //    productToUpdate.QuantityPerUnit = SelectedProduct.QuantityPerUnit;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //        return RedirectToPage();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!ProductsExists(SelectedProduct.ProductID))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}

        private bool ProductsExists(int id)
        {
            return _context.Products.Any(e => e.ProductID == id);
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int productId, int quantity)
        {
            var accountName = User.Identity.Name;
            if (accountName == null)
            {
                return RedirectToPage("/PizzaWeb/Login");
            }
            var account = await _context.Accounts.FirstOrDefaultAsync(u => u.UserName == accountName);
            if (account == null)
            {
                return NotFound();
            }
            var accountID = account.AccountID;

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.Account)
                .FirstOrDefaultAsync(c => c.Account.UserName == accountName);

            if (cart == null)
            {
                cart = new Cart
                {
                    AccountID = accountID,
                    CartItemsJson = "[]"
                };
                _context.Carts.Add(cart);
            }

            if (product.QuantityPerUnit < quantity)
            {
                throw new Exception("Order quantity cannot exceed available stock.");
            }

            var cartItems = JsonConvert.DeserializeObject<List<CartItems>>(cart.CartItemsJson) ?? new List<CartItems>();
            var existingItem = cartItems.FirstOrDefault(ci => ci.ProductID == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cartItems.Add(new CartItems
                {
                    ProductID = productId,
                    Quantity = quantity,
                    Price = product.UnitPrice,
                    ProductName = product.ProductName,
                    ProductImage = product.ProductImage
                });
            }

            product.QuantityPerUnit -= quantity;
            cart.CartItemsJson = JsonConvert.SerializeObject(cartItems);

            await _context.SaveChangesAsync();
            HttpContext.Session.SetInt32("CartItemCount", cartItems.Count());
            return RedirectToPage("/PizzaWeb/Menu");
        }
    }
}
