using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.DTOs.Common;
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
        private const string Title = "title";
        private const string Priority = "priority";
        private const string DueDate = "duedate";
        private const string SortOrderAscending = "asc";
        private const string SortOrderDescending = "desc";

        public TasksController(ToDoContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<ActionResult<PagedResult<TaskItemDto>>> Get(
            [FromQuery] int? userId, 
            [FromQuery] string? category,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string sortOrder = SortOrderAscending)
        {
            var query = _context.Tasks
                .AsNoTracking()
                .Include(t => t.TaskCategories)
                .ThenInclude(tc => tc.Category)
                .AsQueryable();

            // filter by user
            if (userId.HasValue)
            {
                query = query.Where(t => t.UserId == userId.Value);
            }

            // filter by category
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(t => t.TaskCategories.Any(tc => tc.Category.Name == category));
            }
            
            // Sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                query = sortBy.ToLower() switch
                {
                    Title => sortOrder == SortOrderDescending
                        ? query.OrderByDescending(t => t.Title)
                        : query.OrderBy(t => t.Title),

                    Priority => sortOrder == SortOrderDescending
                        ? query.OrderByDescending(t => t.Priority)
                        : query.OrderBy(t => t.Priority),

                    DueDate => sortOrder == SortOrderDescending
                        ? query.OrderByDescending(t => t.DueDate)
                        : query.OrderBy(t => t.DueDate),

                    _ => query
                };
            }
            
            var totalItems = await query.CountAsync();
            
            // paging
            var tasks = await query
                .Skip((pageNumber - 1) * pageSize) // skip number of pages (one page for second page etc.)
                .Take(pageSize) // take only page size number of items 
                .Select(t => t.ToDto())
                .ToListAsync();
            
            // Result
            var result = new PagedResult<TaskItemDto>(
                tasks,
                pageNumber,
                pageSize,
                totalItems
            );

            return Ok(result);
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