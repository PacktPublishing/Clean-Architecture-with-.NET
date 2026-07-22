using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TightlyCoupled.WebShop.Data;

namespace TightlyCoupled.WebShop.ViewComponents;

public class CartNavViewComponent(ApplicationDbContext context) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var itemCount = 0;

        if (!string.IsNullOrEmpty(userId))
        {
            itemCount = await context.CartItems
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Quantity);
        }

        ViewBag.IsAuthenticated = !string.IsNullOrEmpty(userId);
        return View(itemCount);
    }
}
