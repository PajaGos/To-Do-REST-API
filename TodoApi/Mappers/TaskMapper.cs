using TodoApi.DTOs.Tasks;
using TodoApi.Models;

namespace TodoApi.Mappers
{
    public static class TaskMapper
    {
        public static TaskItemDto ToDto(this TaskItem task)
        {
            return new TaskItemDto
            {
                Title = task.Title,
                IsCompleted = task.IsCompleted,
                Description = task.Description,
                Priority = task.Priority,
                DueDate = task.DueDate,
                User = task.User?.ToDto(),
                Categories = task.TaskCategories.Select(tc => tc.Category.ToDto()).ToList(),
            };
        }

        public static TaskItem ToEntity(this TaskItemCreateDto dto)
        {
            return new TaskItem
            {
                Title = dto.Title,
                IsCompleted = dto.IsCompleted,
                Description =  dto.Description,
                Priority = dto.Priority,
                DueDate = dto.DueDate,
                UserId = dto.UserId,
            };
        }

        public static void UpdateFromDto(this TaskItem task, TaskItemUpdateDto dto)
        {
            if (dto.Title != null)
            {
                task.Title = dto.Title;
            }
            if (dto.IsCompleted.HasValue)
            {
                task.IsCompleted = dto.IsCompleted.Value;
            }
            if(dto.Description != null)
            {
                task.Description = dto.Description;
            }
            if (dto.Priority.HasValue)
            {
                task.Priority = dto.Priority.Value;
            }
            if (dto.DueDate.HasValue)
            {
                task.DueDate = dto.DueDate.Value;
            }
            if (dto.UserId.HasValue)
            {
                task.UserId = dto.UserId.Value;
            }
        }
    }
}