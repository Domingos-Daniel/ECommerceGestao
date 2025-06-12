using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ECommerceGestao.Data;
using ECommerceGestao.Models;
using System.Linq;

namespace ECommerceGestao.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Dashboard
        public async Task<IActionResult> Index()
        {
            // Get counts for dashboard
            ViewBag.ProductCount = await _context.Products.CountAsync();
            ViewBag.OrderCount = await _context.Orders.CountAsync();
            ViewBag.UserCount = await _context.Users.CountAsync();
            
            // Get recent orders
            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();
                
            // Get low stock products
            var lowStockProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Stock < 10)
                .OrderBy(p => p.Stock)
                .Take(5)
                .ToListAsync();
                  // Calculate total sales - use ToListAsync and Sum client-side for SQLite compatibility
            var orders = await _context.Orders
                .Where(o => o.Status != "Cancelled")
                .ToListAsync();
                
            var totalSales = orders.Sum(o => o.TotalAmount);
                
            ViewBag.TotalSales = totalSales;
            ViewBag.RecentOrders = recentOrders;
            ViewBag.LowStockProducts = lowStockProducts;

            return View();
        }

        // GET: Dashboard/Products
        public async Task<IActionResult> Products()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();
                
            return View(products);
        }

        // GET: Dashboard/Orders
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
                
            return View(orders);
        }

        // GET: Dashboard/OrderDetails/5
        public async Task<IActionResult> OrderDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Dashboard/UpdateOrderStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            
            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(OrderDetails), new { id = id });
        }

        // GET: Dashboard/Categories
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();
                
            return View(categories);
        }

        // GET: Dashboard/Reports
        public async Task<IActionResult> Reports()
        {            // Sales by category - fetch data and group on client side for SQLite compatibility
            var orderItems = await _context.OrderItems
                .Include(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .ToListAsync();
                
            var salesByCategory = orderItems
                .Where(oi => oi.Product?.Category != null)
                .GroupBy(oi => oi.Product.Category.Name)
                .Select(g => new
                {
                    CategoryName = g.Key,
                    TotalSales = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .ToList();
                  // Sales by month (for the current year) - fetch data and group on client side for SQLite compatibility
            var currentYear = DateTime.Now.Year;
            var ordersCurrentYear = await _context.Orders
                .Where(o => o.OrderDate.Year == currentYear && o.Status != "Cancelled")
                .ToListAsync();
                
            var salesByMonth = ordersCurrentYear
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    TotalSales = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(x => x.Month)
                .ToList();
                  // Top selling products - fetch data and group on client side for SQLite compatibility
            var allOrderItems = await _context.OrderItems
                .Include(oi => oi.Product)
                .ToListAsync();
                
            var topProducts = allOrderItems
                .Where(oi => oi.Product != null)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    ProductName = g.First().Product.Name,
                    TotalQuantity = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(10)
                .ToList();

            ViewBag.SalesByCategory = salesByCategory;
            ViewBag.SalesByMonth = salesByMonth;
            ViewBag.TopProducts = topProducts;
            
            return View();
        }
    }
}
