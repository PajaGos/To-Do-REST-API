using TodoApi.DTOs.Category;
using TodoApi.DTOs.Tasks;

namespace TodoApi.DTOs.TaskCategory
{
    public class TaskCategoryDto
    {
        public TaskItemDto Task { get; set; }
        public CategoryDto Category { get; set; }
    }
}