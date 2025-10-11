using TodoApi.Models;

namespace TodoApi.DTOs.Tasks
{
    public class TaskItemUpdateDto
    {
        public string? Title { get; set; }
        public bool? IsCompleted { get; set; }
        public string? Description { get; set; }
        public PriorityLevel? Priority { get; set; } 
        public DateTime? DueDate { get; set; }
        
        public int? UserId { get; set; } // optional reassignment
        public List<int>? TaskCategories { get; set; } // optional update categories
    }
}