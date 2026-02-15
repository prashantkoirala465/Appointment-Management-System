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
    /// API controller for managing navigation menus
    /// All operations restricted to Admin role
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    public class MenusApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MenusApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// GET: api/menusapi
        /// Returns all menus ordered by display order
        [HttpGet]
        [ProducesResponseType(typeof(List<MenuDto>), 200)]
        public async Task<ActionResult<List<MenuDto>>> GetAll()
        {
            var menus = await _context.Menus
                .OrderBy(m => m.DisplayOrder)
                .Select(m => new MenuDto
                {
                    Id = m.Id,
                    MenuName = m.MenuName,
                    Url = m.Url,
                    DisplayOrder = m.DisplayOrder,
                    IsActive = m.IsActive
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
        /// Permanently deletes a menu item and its user assignments
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null) return NotFound(new { message = "Menu not found." });

            var userMenus = await _context.UserMenus.Where(um => um.MenuId == id).ToListAsync();
            _context.UserMenus.RemoveRange(userMenus);
            _context.Menus.Remove(menu);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
