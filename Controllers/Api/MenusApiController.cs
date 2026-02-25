using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppointmentSystem.Web.Data;
using AppointmentSystem.Web.Models;
using AppointmentSystem.Web.Models.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Web.Controllers.Api
{
    /// REST API controller for managing navigation menu items (Ananta's module)
    /// Menus define what sidebar navigation items exist ("Dashboard", "Appointments", etc.)
    /// They're assigned to individual users through the UserMenus junction table
    /// All operations require Admin role
    [ApiController]                       // automatic validation + JSON errors
    [Route("api/[controller]")]           // base: /api/menusapi
    [Authorize(Policy = "Admin")]         // admin-only
    [Produces("application/json")]        // JSON responses
    public class MenusApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // database access

        public MenusApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// GET: api/menusapi
        /// Returns all menu items sorted by display order (the order they appear in the sidebar)
        [HttpGet]
        [ProducesResponseType(typeof(List<MenuDto>), 200)]
        public async Task<ActionResult<List<MenuDto>>> GetAll()
        {
            var menus = await _context.Menus
                .OrderBy(m => m.DisplayOrder)              // maintain sidebar ordering
                .Select(m => new MenuDto                   // project into DTO
                {
                    Id = m.Id,
                    MenuName = m.MenuName,                 // display name ("Appointments")
                    Url = m.Url,                           // where it links to ("/Appointments")
                    DisplayOrder = m.DisplayOrder,         // sort position in the sidebar
                    IsActive = m.IsActive                  // whether it shows up at all
                })
                .ToListAsync();

            return Ok(menus);
        }

        /// GET: api/menusapi/{id}
        /// Returns a single menu by ID
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MenuDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<MenuDto>> GetById(Guid id)
        {
            var m = await _context.Menus.FindAsync(id);
            if (m == null) return NotFound(new { message = "Menu not found." });

            return Ok(new MenuDto
            {
                Id = m.Id,
                MenuName = m.MenuName,
                Url = m.Url,
                DisplayOrder = m.DisplayOrder,
                IsActive = m.IsActive
            });
        }

        /// POST: api/menusapi
        /// Creates a new menu item
        [HttpPost]
        [ProducesResponseType(typeof(MenuDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<MenuDto>> Create([FromBody] MenuCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var menu = new Menu
            {
                Id = Guid.NewGuid(),
                MenuName = dto.MenuName,
                Url = dto.Url,
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive
            };

            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            var result = new MenuDto
            {
                Id = menu.Id,
                MenuName = menu.MenuName,
                Url = menu.Url,
                DisplayOrder = menu.DisplayOrder,
                IsActive = menu.IsActive
            };

            return CreatedAtAction(nameof(GetById), new { id = menu.Id }, result);
        }

        /// PUT: api/menusapi/{id}
        /// Updates an existing menu item
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MenuDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<MenuDto>> Update(Guid id, [FromBody] MenuCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var menu = await _context.Menus.FindAsync(id);
            if (menu == null) return NotFound(new { message = "Menu not found." });

            menu.MenuName = dto.MenuName;
            menu.Url = dto.Url;
            menu.DisplayOrder = dto.DisplayOrder;
            menu.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new MenuDto
            {
                Id = menu.Id,
                MenuName = menu.MenuName,
                Url = menu.Url,
                DisplayOrder = menu.DisplayOrder,
                IsActive = menu.IsActive
            });
        }

        /// DELETE: api/menusapi/{id}
        /// Permanently removes a menu item and all user-menu assignments for it
        /// Must clean up UserMenus junction table first (FK constraint)
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]   // 204 = deleted
        [ProducesResponseType(404)]   // 404 = menu not found
        public async Task<IActionResult> Delete(Guid id)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null) return NotFound(new { message = "Menu not found." });

            // remove all user-menu links first — FK constraint requires this
            var userMenus = await _context.UserMenus.Where(um => um.MenuId == id).ToListAsync();
            _context.UserMenus.RemoveRange(userMenus);

            // now safely delete the menu item itself
            _context.Menus.Remove(menu);
            await _context.SaveChangesAsync();

            return NoContent();  // 204 = gone
        }
    }
}
