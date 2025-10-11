using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
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
        public async Task<ActionResult<IEnumerable<TaskItem>>> Get([FromQuery] int? userId, [FromQuery] string? category)
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

            var tasks = await query.ToListAsync();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetById(int id)
        {
            var task = await _context.Tasks
                .AsNoTracking()
                .Include(t => t.TaskCategories)
                .ThenInclude(tc => tc.Category)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            return task == null ? NotFound() : Ok(task);
        }

        [HttpPost]
        public async Task<ActionResult<TaskItem>> Create(TaskItem task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TaskItem updatedTask)
        {
            if (id != updatedTask.Id)
            {
                return BadRequest();
            }
            
            var existingTask = await _context.Tasks
                .Include(t => t.TaskCategories)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (existingTask == null)
            {
                return NotFound();
            }
            
            existingTask.UpdateFrom(updatedTask);

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