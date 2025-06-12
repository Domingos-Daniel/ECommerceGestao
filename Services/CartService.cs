using ECommerceGestao.Data;
using ECommerceGestao.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceGestao.Services
{
    public class CartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }        public string GetCartId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return Guid.NewGuid().ToString();
            }
            
            var session = httpContext.Session;
            string? cartId = session.GetString("CartId");

            if (string.IsNullOrEmpty(cartId))
            {
                cartId = Guid.NewGuid().ToString();
                session.SetString("CartId", cartId);
            }

            return cartId;
        }

        public async Task AddToCartAsync(int productId, int quantity = 1)
        {
            string cartId = GetCartId();
            
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.CartId == cartId && c.ProductId == productId);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    CartId = cartId,
                    ProductId = productId,
                    Quantity = quantity,
                    DateCreated = DateTime.Now
                };
                _context.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> RemoveFromCartAsync(int id)
        {
            string cartId = GetCartId();
            
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == id && c.CartId == cartId);

            if (cartItem == null)
                return 0;

            _context.CartItems.Remove(cartItem);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateCartQuantityAsync(int id, int quantity)
        {
            string cartId = GetCartId();
            
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == id && c.CartId == cartId);

            if (cartItem == null)
                return 0;

            if (quantity <= 0)
                return await RemoveFromCartAsync(id);

            cartItem.Quantity = quantity;
            return await _context.SaveChangesAsync();
        }

        public async Task<int> EmptyCartAsync()
        {
            string cartId = GetCartId();

            var cartItems = await _context.CartItems
                .Where(c => c.CartId == cartId)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            return await _context.SaveChangesAsync();
        }        public async Task<List<CartItem>> GetCartItemsAsync()
        {
            string cartId = GetCartId();
            
            // Garantir que apenas itens com produtos válidos sejam retornados
            var cartItems = await _context.CartItems
                .Where(c => c.CartId == cartId)
                .Include(c => c.Product)
                    .ThenInclude(p => p.Category)
                .ToListAsync();
                
            // Remover itens inválidos automaticamente (produto foi excluído)
            var invalidItems = cartItems.Where(c => c.Product == null).ToList();
            if (invalidItems.Any())
            {
                _context.CartItems.RemoveRange(invalidItems);
                await _context.SaveChangesAsync();
                
                // Filtrar da lista antes de retornar
                cartItems = cartItems.Where(c => c.Product != null).ToList();
            }
            
            return cartItems;
        }        public async Task<decimal> GetCartTotalAsync()
        {
            string cartId = GetCartId();
            
            // Use ToListAsync and then calculate Sum on the client side instead of using SumAsync
            // This is because SQLite cannot apply Sum() to decimal types directly
            var cartItems = await _context.CartItems
                .Where(c => c.CartId == cartId && c.Product != null)
                .Include(c => c.Product)
                .ToListAsync();
                
            return cartItems.Sum(c => c.Product.Price * c.Quantity);
        }

        public async Task<int> GetCartItemCountAsync()
        {
            string cartId = GetCartId();
            
            return await _context.CartItems
                .Where(c => c.CartId == cartId)
                .CountAsync();
        }
    }
}
