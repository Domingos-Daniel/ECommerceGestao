using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ECommerceGestao.Data;
using ECommerceGestao.Models;
using ECommerceGestao.Services;
using Microsoft.AspNetCore.Identity;

namespace ECommerceGestao.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(
            ApplicationDbContext context,
            CartService cartService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _cartService = cartService;
            _userManager = userManager;
        }

        // GET: Order/Checkout
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var cartItems = await _cartService.GetCartItemsAsync();
            if (cartItems.Count == 0)
            {
                TempData["Error"] = "Seu carrinho está vazio.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);
            ViewBag.CartTotal = await _cartService.GetCartTotalAsync();
            
            return View();
        }

        // POST: Order/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> PlaceOrder(string shippingAddress, string paymentMethod)
        {
            var cartItems = await _cartService.GetCartItemsAsync();
            if (cartItems.Count == 0)
            {
                TempData["Error"] = "Seu carrinho está vazio.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }            // Create order
            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                Status = "Pendente",
                TotalAmount = await _cartService.GetCartTotalAsync(),
                ShippingAddress = shippingAddress,
                PaymentMethod = paymentMethod,
                OrderItems = new List<OrderItem>()
            };

            // Add order items
            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price
                };
                
                order.OrderItems.Add(orderItem);
            }

            // Save order to database
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Update stock
            foreach (var item in cartItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Stock -= item.Quantity;
                    _context.Entry(product).State = EntityState.Modified;
                }
            }
            await _context.SaveChangesAsync();

            // Empty cart
            await _cartService.EmptyCartAsync();

            TempData["Success"] = "Pedido realizado com sucesso!";
            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }        // GET: Order/OrderConfirmation/5
        [Authorize]
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems!)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || (user.Id != order.UserId && !User.IsInRole("Admin")))
            {
                return Forbid();
            }

            // Se for um método de pagamento que precisa de instruções adicionais, redirecionar para PaymentInfo
            if (order.PaymentMethod == "Cartão Multicaixa" || 
                order.PaymentMethod == "Transferência Bancária" || 
                order.PaymentMethod == "e-Kwanza")
            {
                return RedirectToAction("PaymentInfo", new { id = order.Id });
            }

            return View(order);
        }// GET: Order/PaymentInfo/5
        [Authorize]
        public async Task<IActionResult> PaymentInfo(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems!)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || (user.Id != order.UserId && !User.IsInRole("Admin")))
            {
                return Forbid();
            }

            return View(order);
        }

        // GET: Order/MyOrders
        [Authorize]        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return Challenge();
            }

            var orders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Order/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }            var user = await _userManager.GetUserAsync(User);
            if (user == null || (user.Id != order.UserId && !User.IsInRole("Admin")))
            {
                return Forbid();
            }

            return View(order);
        }
    }
}
