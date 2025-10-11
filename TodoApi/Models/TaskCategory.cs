namespace TodoApi.Models
{
    public class TaskCategory
    {
        public int TaskId { get; set; }
        public TaskItem Task { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
        
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}