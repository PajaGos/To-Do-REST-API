using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.DTOs.Tasks;
using TodoApi.Mappers;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("tasks")]
    public class TasksController : ControllerBase
    {
        private readonly ToDoContext _context;

        public TasksController(ToDoContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItemDto>>> Get([FromQuery] int? userId, [FromQuery] string? category)
        {
            var query = _context.Tasks
                .AsNoTracking()
                .Include(t => t.TaskCategories)
                .ThenInclude(tc => tc.Category)
                .AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(t => t.UserId == userId.Value);
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(t => t.TaskCategories.Any(tc => tc.Category.Name == category));
            }

            var tasks = await query.Select(x => x.ToDto()).ToListAsync();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItemDto>> GetById(int id)
        {
            var task = await _context.Tasks
                .AsNoTracking()
                .Include(t => t.TaskCategories)
                .ThenInclude(tc => tc.Category)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            return task == null ? NotFound() : Ok(task.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<TaskItemDto>> Create(TaskItemCreateDto dto)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
            {
                return BadRequest($"User with ID {dto.UserId} does not exist.");
            }

            var task = dto.ToEntity();
            
            // Link categories (if provided)
            if (dto.CategoryIds.Any())
            {
                var existingCategories = await _context.Categories
                    .Where(c => dto.CategoryIds.Contains(c.Id))
                    .ToListAsync();
                
                if(existingCategories.Count != dto.CategoryIds.Count)
                {
                    return BadRequest("One or more provided category IDs do not exist.");
                }

                task.TaskCategories = existingCategories
                    .Select(c => new TaskCategory
                    {
                        CategoryId = c.Id,
                        Task = task
                    }).ToList();
            }
            
            _context.Tasks.Add(task);
            
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = task.Id }, task.ToDto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TaskItemUpdateDto dto)
        {
            var existingTask = await _context.Tasks
                .Include(t => t.TaskCategories)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (existingTask == null)
            {
                return NotFound();
            }
            
            var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
            {
                return BadRequest($"User with ID {dto.UserId} does not exist.");
            }
            
            existingTask.UpdateFromDto(dto);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}