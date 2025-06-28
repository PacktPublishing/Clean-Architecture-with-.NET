using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TightlyCoupled.WebShop.Data;

namespace TightlyCoupled.WebShop.ViewComponents
{
    public class CartNavViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CartNavViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var itemCount = 0;

            if (!string.IsNullOrEmpty(userId))
            {
                itemCount = await _context.CartItems
                    .Where(c => c.UserId == userId)
                    .SumAsync(c => c.Quantity);
            }

            ViewBag.IsAuthenticated = !string.IsNullOrEmpty(userId);
            return View(itemCount);
        }
    }
}
