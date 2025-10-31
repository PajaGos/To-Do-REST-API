using TodoApi.DTOs.TaskCategory;
using TodoApi.Models;

namespace TodoApi.Mappers
{
    public static class TaskCategoryMapper
    {
        public static TaskCategoryDto ToDto(this TaskCategory taskCategory)
        {
            return new TaskCategoryDto
            {
                Task = taskCategory.Task.ToDto(), 
                Category = taskCategory.Category.ToDto(),
            };
        }
    }
}