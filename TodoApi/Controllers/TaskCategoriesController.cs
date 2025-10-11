using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
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
        public async Task<IActionResult> GetCategoriesForTask(int taskId)
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

            return Ok(task.TaskCategories.Select(tc => new
            {
                tc.CategoryId,
                tc.Category.Name,
                tc.AssignedAt
            }));
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> AssignCategory(int taskId, int categoryId)
        {
            var exists = await _context.TaskCategories
                .AnyAsync(tc => tc.TaskId == taskId && tc.CategoryId == categoryId);

            if (exists)
            {
                return Conflict("This category is already assigned to the task.");
            }

            _context.TaskCategories.Add(new TaskCategory
            {
                TaskId = taskId,
                CategoryId = categoryId,
                AssignedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveCategory(int taskId, int categoryId)
        {
            var link = await _context.TaskCategories
                .FirstOrDefaultAsync(tc => tc.TaskId == taskId && tc.CategoryId == categoryId);

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