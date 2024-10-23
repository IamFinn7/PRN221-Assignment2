using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PizzaStore.Models;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using PizzaStore.DTO;
using Newtonsoft.Json;

namespace PizzaOrderWebsite.Pages.PizzaWeb
{
    public class ProductEditModel(PizzaContext context, IMapper mapper, IConfiguration configuration, IUploadImageService uploadImageService) : PageModel
    {
        private readonly PizzaContext _context = context;
        private readonly IMapper _mapper = mapper;
        private readonly IConfiguration _configuration = configuration;
        private readonly IUploadImageService _uploadImageService = uploadImageService;

        public List<Products> Products { get; set; } = new List<Products>();

        [BindProperty]
        public ProductVM SelectedProduct { get; set; } = new ProductVM();
        [BindProperty]
        public IFormFile File { get; set; } = default!;
        public async Task OnGetAsync(int? id)
        {
            if (id.HasValue)
            {
                var product = await _context.Products.FirstOrDefaultAsync(m => m.ProductID == id.Value);
                if (product != null)
                {
                    SelectedProduct = _mapper.Map<ProductVM>(product);
                }
            }

            // Populate ViewData for categories and suppliers
            ViewData["SupplierID"] = new SelectList(_context.Suppliers, "SupplierID", "CompanyName");
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryName");
        }

        public async Task<IActionResult> OnGetEditAsync(int productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(m => m.ProductID == productId);
            if (product == null)
            {
                return NotFound();
            }

            var productVm = _mapper.Map<ProductVM>(product);
            ViewData["SupplierID"] = new SelectList(_context.Suppliers, "SupplierID", "CompanyName");
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryName");
            return new JsonResult(productVm);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var productToUpdate = await _context.Products.FindAsync(SelectedProduct.ProductID);
            if (productToUpdate == null)
            {
                return NotFound();
            }

            productToUpdate.ProductName = SelectedProduct.ProductName;
            productToUpdate.UnitPrice = SelectedProduct.UnitPrice;
            productToUpdate.QuantityPerUnit = SelectedProduct.QuantityPerUnit;
            productToUpdate.SupplierID = SelectedProduct.SupplierID;
            productToUpdate.CategoryID = SelectedProduct.CategoryID;
            if (this.File != null)
            {
                productToUpdate.ProductImage = await _uploadImageService.UploadImage(this.File);
            }
            else
            {
                productToUpdate.ProductImage = SelectedProduct.ProductImage;
            }


            try
            {
                await _context.SaveChangesAsync();
                return RedirectToPage("./Menu");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductsExists(SelectedProduct.ProductID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        private bool ProductsExists(int id)
        {
            return _context.Products.Any(e => e.ProductID == id);
        }

    
    }
}


