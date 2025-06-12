using ECommerceGestao.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceGestao.ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        private readonly CartService _cartService;

        public CartCountViewComponent(CartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var itemCount = await _cartService.GetCartItemCountAsync();
            return View(itemCount);
        }
    }
}
