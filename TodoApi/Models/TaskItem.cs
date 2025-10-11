namespace TodoApi.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
        public string Description { get; set; } = string.Empty;
        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
        public DateTime DueDate { get; set; }
        
        // Foreign key to User (automatically handled by EF Core)
        public int UserId { get; set; }
        public User User { get; set; }

        public List<TaskCategory> TaskCategories { get; set; } = new List<TaskCategory>();
        
        public List<TaskCategory> TaskCategories { get; set; }
        
        public void UpdateFrom(TaskItem other)
        {
            if (other == null)
            {
                return;
            }
            
            Title = other.Title;
            IsCompleted = other.IsCompleted;
            Description = other.Description;
            Priority = other.Priority;
            DueDate = other.DueDate;
            UserId = other.UserId;
            User = other.User;
            TaskCategories = other.TaskCategories;
            
        }
    }

}