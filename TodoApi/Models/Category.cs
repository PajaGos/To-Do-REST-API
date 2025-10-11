namespace TodoApi.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Foreign key (each user can have their own categories)
        public int UserId { get; set; }
        public User User { get; set; }

        public List<TaskCategory> TaskCategories { get; set; }
        
        public void UpdateFrom(Category updateCategory)
        {
            if (updateCategory == null)
            {
                return;
            }
            
            Name = updateCategory.Name;
        }
    }
}