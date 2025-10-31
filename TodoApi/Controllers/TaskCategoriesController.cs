using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.DTOs.Category;
using TodoApi.Mappers;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId}/categories")]
    public class TaskCategoriesController : ControllerBase
    {
        private readonly ToDoContext _context;

        public TaskCategoriesController(ToDoContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategoriesForTask(int taskId)
        {
            var task = await _context.Tasks
                .AsNoTracking() // change tracker won't track the entity since it will be read only (performance optimization)
                .Include(t => t.TaskCategories)
                .ThenInclude(tc => tc.Category)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return NotFound();
            }

            return Ok(task.TaskCategories.Select(tc => tc.ToDto().Category));
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> AssignCategory(int taskId, int id)
        {
            var task = await _context.Tasks
                .AsNoTracking() // change tracker won't track the entity since it will be read only (performance optimization)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return BadRequest($"Task with id {taskId} does not exist.");
            }
            
            var category = await _context.Categories
                .AsNoTracking() // change tracker won't track the entity since it will be read only (performance optimization)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return BadRequest($"Category with id {id} does not exist.");
            }
            
            var exists = await _context.TaskCategories
                .AnyAsync(tc => tc.TaskId == taskId && tc.CategoryId == id);

            if (exists)
            {
                return Conflict("This category is already assigned to the task.");
            }

            _context.TaskCategories.Add(new TaskCategory
            {
                TaskId = taskId,
                CategoryId = id,
            });

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveCategory(int taskId, int id)
        {
            var link = await _context.TaskCategories
                .FirstOrDefaultAsync(tc => tc.TaskId == taskId && tc.CategoryId == id);

            if (link == null)
            {
                return NotFound();
            }

            _context.TaskCategories.Remove(link);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}