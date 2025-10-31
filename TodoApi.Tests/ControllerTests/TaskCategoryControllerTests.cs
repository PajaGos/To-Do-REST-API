using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Controllers;
using TodoApi.Data;
using TodoApi.DTOs.Category;
using TodoApi.Mappers;
using TodoApi.Models;
using Xunit;
namespace TodoApi.Tests
{
    public class TaskCategoryControllerTests : IAsyncLifetime
    {
        private ToDoContext context;
        private List<User> users;
        private List<TaskItem> tasks;
        private List<TaskCategory> taskCategories;
        private List<Category> categories;

        private const int NonExistingId = 150;

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder<ToDoContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // InMemory provider, unique db for each test
                .Options;

            users = new List<User>
            {
                new User { Id = 1, UserName = "TestUser1", Email = "user1@email.com" },
                new User { Id = 2, UserName = "TestUser2", Email = "user2@email.com" },
            };

            var testUser1 = users.First();
            var testUser2 = users.Skip(1).First();

            tasks = new List<TaskItem>
            {
                new TaskItem { Id = 1, Title = "Test1", IsCompleted = false, DueDate = DateTime.MaxValue, User = testUser1, UserId = testUser1.Id },
                new TaskItem { Id = 2, Title = "Test2", IsCompleted = true, DueDate = DateTime.MaxValue, User = testUser1, UserId = testUser1.Id },
                new TaskItem { Id = 3, Title = "Test3", IsCompleted = false, DueDate = DateTime.MaxValue, User = testUser1, UserId = testUser1.Id },

                new TaskItem { Id = 4, Title = "Test4", IsCompleted = false, DueDate = DateTime.MaxValue, User = testUser2, UserId = testUser2.Id },
                new TaskItem { Id = 5, Title = "Test5", IsCompleted = true, DueDate = DateTime.MaxValue, User = testUser2, UserId = testUser2.Id },
            };

            categories = new List<Category>()
            {
                new Category() { Id = 1, Name = "Work" },
                new Category() { Id = 2, Name = "Personal" },
                new Category() { Id = 3, Name = "Urgent" },
            };

            taskCategories = new List<TaskCategory>()
            {
                new TaskCategory { TaskId = 1, CategoryId = 1 },
                new TaskCategory { TaskId = 2, CategoryId = 2 },
                new TaskCategory { TaskId = 3, CategoryId = 1 },

                new TaskCategory { TaskId = 4, CategoryId = 3 },
                new TaskCategory { TaskId = 5, CategoryId = 3 },
            };

            context = new ToDoContext(options);
            context.Tasks.AddRange(tasks);
            context.Users.AddRange(users);
            context.TaskCategories.AddRange(taskCategories);
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }
        
        public Task DisposeAsync()
        {
            context?.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task GetCategoriesForTask_ReturnsTaskCategories_WhenTaskExists()
        {
            // Arrange
            TaskItem task = tasks.First();
            List<CategoryDto> linkedCategories = taskCategories.Where(tc => tc.TaskId == task.Id)
                .Select(x => x.Category.ToDto()).ToList();

            var target = new TaskCategoriesController(context);

            // Act
            var result = await target.GetCategoriesForTask(task.Id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<CategoryDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<CategoryDto>>(okResult.Value);
            Assert.Equivalent(linkedCategories, returnValue);
        }

        [Fact]
        public async Task GetCategoriesForTask_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var target = new TaskCategoriesController(context);

            // Act
            var result = await target.GetCategoriesForTask(NonExistingId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<CategoryDto>>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }
        
        [Fact]
        public async Task AssignCategory_ReturnsNoContent_WhenAssignIsCompleted()
        {
            var task = tasks.First();
            var category = categories.Last();
            
            // Arrange
            var target = new TaskCategoriesController(context);

            // Act
            var result = await target.AssignCategory(task.Id, category.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
        
        [Fact]
        public async Task AssignCategory_CategoryIsAssigned_WhenAssignIsCompleted()
        {
            var task = tasks.First();
            var category = categories.Last();
            
            // Arrange
            var target = new TaskCategoriesController(context);

            // Act
            var result = await target.AssignCategory(task.Id, category.Id);
            var taskResult = await context.Tasks
                .AsNoTracking() // change tracker won't track the entity since it will be read only (performance optimization)
                .Include(t => t.TaskCategories)
                .ThenInclude(tc => tc.Category)
                .FirstOrDefaultAsync(t => t.Id == task.Id);
            
            // Assert
            Assert.Contains(taskResult.TaskCategories, x => x.CategoryId == category.Id);
        }
        
        [Fact]
        public async Task AssignCategory_ReturnsConflict_WhenCategoryIsAlreadyAssigned()
        {
            var task = tasks.First();
            var category = categories.First();
            
            // Arrange
            var target = new TaskCategoriesController(context);

            // Act
            var result = await target.AssignCategory(task.Id, category.Id);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal("This category is already assigned to the task.", conflictResult.Value);
        }
        
        [Fact]
        public async Task AssignCategory_ReturnsBadRequest_WhenCategoryDoesNotExist()
        {
            var task = tasks.First();
            
            // Arrange
            var target = new TaskCategoriesController(context);

            // Act
            var result = await target.AssignCategory(task.Id, NonExistingId);

            // Assert
            var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"Category with id {NonExistingId} does not exist.", badRequestObjectResult.Value);
        }
        
        [Fact]
        public async Task AssignCategory_ReturnsBadRequest_WhenTaskDoesNotExist()
        {
            // Arrange
            var target = new TaskCategoriesController(context);

            // Act
            var result = await target.AssignCategory(NonExistingId, categories.First().Id);

            // Assert
            var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"Task with id {NonExistingId} does not exist.", badRequestObjectResult.Value);
        }
        
        [Fact]
        public async Task RemoveCategory_ReturnsNoContent_WhenCategoryIsRemoved()
        {
            var taskCategory = taskCategories.First();
            
            // Arrange
            var target = new TaskCategoriesController(context);

            // Act
            var result = await target.RemoveCategory(taskCategory.TaskId, taskCategory.CategoryId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
        
        [Fact]
        public async Task RemoveCategory_TaskIsNotInCategory_WhenCategoryIsRemoved()
        {
            var taskCategory = taskCategories.First();
            
            // Arrange
            var target = new TaskCategoriesController(context);

            // Act
            var result = await target.RemoveCategory(taskCategory.TaskId, taskCategory.CategoryId);
            var task = await context.Tasks
                .AsNoTracking() // change tracker won't track the entity since it will be read only (performance optimization)
                .Include(t => t.TaskCategories)
                .ThenInclude(tc => tc.Category)
                .FirstOrDefaultAsync(t => t.Id == taskCategory.TaskId);
            
            // Assert
            Assert.DoesNotContain(task.TaskCategories, x => x.CategoryId == taskCategory.CategoryId);
        }
        
        [Fact]
        public async Task RemoveCategory_ReturnsNotFound_WhenCategoryIsNotAssignedToTask()
        {
            var taskCategory = taskCategories.First();
            
            // Arrange
            var target = new TaskCategoriesController(context);

            // Act
            var result = await target.RemoveCategory(taskCategory.TaskId, NonExistingId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}