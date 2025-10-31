using TodoApi.DTOs.Category;
using TodoApi.DTOs.Users;
using TodoApi.Models;

namespace TodoApi.DTOs.Tasks
{
    public class TaskItemDto
    {
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
        public string Description { get; set; } = string.Empty;
        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
        public DateTime DueDate { get; set; }
        
        public UserDto? User { get; set; }
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
    }
}