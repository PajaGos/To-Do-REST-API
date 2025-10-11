using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly ToDoContext _context;
        
        public UsersController(ToDoContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            return Ok(await _context.Users.AsNoTracking().ToListAsync());
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return user == null ? NotFound() : Ok(user);
        }
        
        [HttpGet("{id}/tasks")]
        public async Task<IActionResult> GetTasksForUser(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Tasks)
                .ThenInclude(t => t.TaskCategories)
                .ThenInclude(tc => tc.Category)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var tasks = user.Tasks.Select(t => new
            {
                t.Id,
                t.Title,
                t.IsCompleted,
                t.Description,
                t.Priority,
                t.DueDate,
                Categories = t.TaskCategories.Select(tc => new { tc.CategoryId, tc.Category.Name })
            });

            return Ok(tasks);
        }

        [HttpPost]
        public async Task<ActionResult<User>> Create(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }
            
            User? existingUser = await _context.Users.FirstOrDefaultAsync(t => t.Id == id);

            if (existingUser == null)
            {
                return NotFound();
            }

            existingUser.UpdateFrom(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}