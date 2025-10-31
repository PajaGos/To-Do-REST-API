using System.ComponentModel.DataAnnotations;
using TodoApi.Models;

namespace TodoApi.DTOs.Tasks
{
    public class TaskItemCreateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Title { get; set; }
        public bool IsCompleted { get; set; } = false;
        public string Description { get; set; } = string.Empty;
        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
        public DateTime DueDate { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        // Optional list of category IDs to assign the task to
        public List<int> CategoryIds { get; set; } = new List<int>();
    }
}