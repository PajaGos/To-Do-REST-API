using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.DTOs.Tasks;
using TodoApi.DTOs.Users;
using TodoApi.Mappers;

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
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            return Ok(await _context.Users.AsNoTracking().Select(x => x.ToDto()).ToListAsync());
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return user == null ? NotFound() : Ok(user.ToDto());
        }
        
        [HttpGet("{id}/tasks")]
        public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasksForUser(int id)
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

            IEnumerable<TaskItemDto> tasks = user.Tasks.Select(t => t.ToDto());
            return Ok(tasks);
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> Create(UserCreateDto dto)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == dto.UserName);

            if (user != null)
            {
                return Conflict($"Username {dto.UserName} already exists.");
            }
            
            var userEmail = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (userEmail != null)
            {
                return Conflict($"Username with the same email {dto.Email} already exists.");
            }

            var entity = dto.ToEntity();
            _context.Users.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity.ToDto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UserUpdateDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            user.UpdateFromDto(dto);
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