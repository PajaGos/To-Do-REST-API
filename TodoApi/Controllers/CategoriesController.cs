using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.DTOs.Category;
using TodoApi.Mappers;

namespace TodoApi.Controllers;

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
            .Select(x => x.ToDto())
            .ToListAsync();

        return Ok(categories);
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
        
        return Ok(category.ToDto());
    }
    
    [HttpGet("{id}/tasks")]
    public async Task<IActionResult> GetTasksForCategory(int id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Include(c => c.TaskCategories)
            .ThenInclude(tc => tc.Task)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        var tasks = category.TaskCategories.Select(tc => tc.ToDto());
        return Ok(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CategoryCreateDto dto)
    {
        // Check that the user exists
        var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
        if (!userExists)
        {
            return BadRequest($"User with ID {dto.UserId} does not exist.");
        }
        
        var categoryExists = await _context.Categories.AnyAsync(u => u.Name == dto.Name);
        if (categoryExists)
        {
            return Conflict($"Category with name {dto.Name} already exists.");
        }
        
        var category = dto.ToEntity();
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category.ToDto());
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CategoryUpdateDto dto)
    {
        var categoryExists = await _context.Categories.AnyAsync(u => u.Name == dto.Name);
        if (!categoryExists)
        {
            return Conflict($"Category with name {dto.Name} already exists.");
        }
            
        var existingCategory = await _context.Categories.FirstOrDefaultAsync(t => t.Id == id);
        if (existingCategory == null)
        {
            return NotFound();
        }
            
        existingCategory.UpdateFromDto(dto);

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