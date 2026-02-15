using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AppointmentSystem.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Web.Filters
{
    /// This action filter runs before every controller action
    /// It loads the currently logged-in user's assigned menu items from the database
    /// and puts them in ViewData so the layout can render dynamic navigation
    /// This way, each user only sees the menu items they have been granted access to
    public class MenuLoaderFilter : IAsyncActionFilter
    {
        // Database context for querying user menu assignments
        private readonly ApplicationDbContext _context;

        // Constructor: receives the database context via dependency injection
        public MenuLoaderFilter(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Check if the current user is authenticated
            if (context.HttpContext.User.Identity != null && context.HttpContext.User.Identity.IsAuthenticated)
            {
                // Get the user's ID from their claims (stored during login)
                var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    // Load the menu items assigned to this user
                    // We join through UserMenus to get only the menus this specific user has access to
                    // Only load active menus, sorted by DisplayOrder for consistent navigation
                    var userMenus = await _context.UserMenus
                        .Where(um => um.UserId == userId)
                        .Include(um => um.Menu)
                        .Where(um => um.Menu != null && um.Menu.IsActive)
                        .OrderBy(um => um.Menu!.DisplayOrder)
                        .Select(um => um.Menu!)
                        .ToListAsync();

                    // Store the menus in ViewData so the layout can access them
                    // The layout checks for ViewData["UserMenus"] to render the navigation
                    if (context.Controller is Controller controller)
                    {
                        controller.ViewData["UserMenus"] = userMenus;
                    }
                }
            }

            // Continue to the actual controller action
            await next();
        }
    }
}
