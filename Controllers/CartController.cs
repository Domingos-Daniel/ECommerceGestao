using Microsoft.AspNetCore.Mvc;
using ECommerceGestao.Data;
using ECommerceGestao.Models;
using ECommerceGestao.Services;
using Microsoft.EntityFrameworkCore;

namespace ECommerceGestao.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public CartController(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var cartItems = await _cartService.GetCartItemsAsync();
            return View(cartItems);
        }

        // POST: Cart/AddToCart/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            // Validate product exists
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            // Add to cart
            await _cartService.AddToCartAsync(productId, quantity);
            
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/RemoveFromCart/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            await _cartService.RemoveFromCartAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/UpdateQuantity/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            if (quantity <= 0)
            {
                await _cartService.RemoveFromCartAsync(id);
            }
            else
            {
                await _cartService.UpdateCartQuantityAsync(id, quantity);
            }
            
            return RedirectToAction(nameof(Index));
        }        // POST: Cart/ClearCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            await _cartService.EmptyCartAsync();
            return RedirectToAction(nameof(Index));
        }
        
        // GET: Cart/GetCartCount
        [HttpGet]
        public async Task<JsonResult> GetCartCount()
        {
            int count = await _cartService.GetCartItemCountAsync();
            return Json(count);
        }
    }
}
