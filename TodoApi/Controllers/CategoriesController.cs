using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ToDoContext _context;

    public CategoriesController(ToDoContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
        
        if (category == null)
        {
            return NotFound();
        }
        
        return Ok(category);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Category updateCategory)
    {
        if (id != updateCategory.Id)
        {
            return BadRequest();
        }
            
        var existingCategory = await _context.Categories.FirstOrDefaultAsync(t => t.Id == id);
        if (existingCategory == null)
        {
            return NotFound();
        }
            
        existingCategory.UpdateFrom(updateCategory);

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
    
    [HttpGet("{id}/tasks")]
    public async Task<IActionResult> GetTasksForCategory(int id)
    {
        var category = await _context.Categories
            .Include(c => c.TaskCategories)
            .ThenInclude(tc => tc.Task)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        var tasks = category.TaskCategories.Select(tc => new
        {
            tc.Task.Id,
            tc.Task.Title,
            tc.Task.IsCompleted,
            tc.Task.Description,
            tc.Task.Priority,
            tc.Task.DueDate,
        });

        return Ok(tasks);
    }

}