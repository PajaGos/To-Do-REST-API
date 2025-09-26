namespace TodoApi.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool Done { get; set; }
        
        public void UpdateFrom(TaskItem other)
        {
            if (other == null)
            {
                return;
            }
            
            Title = other.Title;
            Done = other.Done;
        }
    }
}