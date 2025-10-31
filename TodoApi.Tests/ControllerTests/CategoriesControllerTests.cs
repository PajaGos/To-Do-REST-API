using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Controllers;
using TodoApi.Data;
using TodoApi.DTOs.Category;
using TodoApi.DTOs.Tasks;
using TodoApi.Mappers;
using TodoApi.Models;
using Xunit;

namespace TodoApi.Tests
{
    public class CategoriesControllerTests : IAsyncLifetime
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
        public async Task GetAll_ReturnsAllCategories_WhenSucceeds()
        {
            // Arrange
            var target = new CategoriesController(context);

            // Act
            var result = await target.GetAll();
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<CategoryDto>>>(result);
            var createdAtActionResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<CategoryDto>>(createdAtActionResult.Value);
            Assert.Equivalent(categories.Select(x => x.ToDto()), returnValue);
        }
        
        [Fact]
        public async Task GetById_ReturnsCategory_WhenFound()
        {
            // Arrange
            Category category = categories.First();
            var target = new CategoriesController(context);

            // Act
            var result = await target.GetById(category.Id);
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<CategoryDto>>(result);
            var createdAtActionResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnValue = Assert.IsAssignableFrom<CategoryDto>(createdAtActionResult.Value);
            Assert.Equivalent(category.ToDto(), returnValue);
        }
        
        [Fact]
        public async Task GetById_ReturnsNotFound_WhenCategoryDoesNotExist()
        {
            // Arrange
            var target = new CategoriesController(context);

            // Act
            var result = await target.GetById(NonExistingId);
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<CategoryDto>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }
        
        [Fact]
        public async Task GetTasksForCategory_ReturnsTasks_WhenFound()
        {
            // Arrange
            Category category = categories.First();
            IEnumerable<TaskItemDto> tasksInCategory = taskCategories
                .Where(tc => tc.CategoryId == category.Id)
                .Select(tc => tc.Task.ToDto());
            var target = new CategoriesController(context);

            // Act
            var result = await target.GetTasksForCategory(category.Id);
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TaskItemDto>>>(result);
            var createdAtActionResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<TaskItemDto>>(createdAtActionResult.Value);
            Assert.Equivalent(tasksInCategory, returnValue);
        } 
        
        [Fact]
        public async Task GetTasksForCategory_ReturnsNotFound_WhenCategoryDoesNotExist()
        {
            // Arrange
            var target = new CategoriesController(context);

            // Act
            var result = await target.GetTasksForCategory(NonExistingId);
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TaskItemDto>>>(result);
            var createdAtActionResult = Assert.IsType<NotFoundResult>(actionResult.Result);
        } 
        
        [Fact]
        public async Task Create_ReturnsCategory_WhenSuccessfullyCreated()
        {
            // Arrange
            CategoryCreateDto dto = new CategoryCreateDto { Name = "NewCategory", UserId = users.First().Id };
            var target = new CategoriesController(context);

            // Act
            var result = await target.Create(dto);
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<CategoryDto>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsAssignableFrom<CategoryDto>(createdAtActionResult.Value);
            Assert.Equal(dto.Name, returnValue.Name);
        } 
        
        [Fact]
        public async Task Create_ReturnsBadRequest_WhenUserDoesNotExist()
        {
            // Arrange
            CategoryCreateDto dto = new CategoryCreateDto { Name = "NewCategory", UserId = NonExistingId };
            var target = new CategoriesController(context);

            // Act
            var result = await target.Create(dto);
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<CategoryDto>>(result);
            var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal($"User with ID {NonExistingId} does not exist.", badRequestObjectResult.Value);
        } 
        
        [Fact]
        public async Task Create_ReturnsConflict_WhenConflictAlreadyExists()
        {
            // Arrange
            CategoryCreateDto dto = new CategoryCreateDto { Name = categories.First().Name, UserId = users.First().Id };
            var target = new CategoriesController(context);

            // Act
            var result = await target.Create(dto);
            
            // Assert
            var actionResult = Assert.IsType<ActionResult<CategoryDto>>(result);
            var requestObjectResult = Assert.IsType<ConflictObjectResult>(actionResult.Result);
            Assert.Equal($"Category with name {dto.Name} already exists.", requestObjectResult.Value);
        } 
        
        [Fact]
        public async Task Update_ReturnsNoContent_WhenCategoryIsUpdated()
        {
            // Arrange
            var category = categories.First();
            CategoryUpdateDto dto = new CategoryUpdateDto { Name = "UpdatedName"};
            var target = new CategoriesController(context);

            // Act
            var result = await target.Update(category.Id, dto);
            
            // Assert
            Assert.IsType<NoContentResult>(result);
        } 
        
        [Fact]
        public async Task Update_ReturnsNoContent_WhenCategoryIsNotChanged()
        {
            // Arrange
            var category = categories.First();
            CategoryUpdateDto dto = new CategoryUpdateDto();
            var target = new CategoriesController(context);

            // Act
            var result = await target.Update(category.Id, dto);
            
            // Assert
            Assert.IsType<NoContentResult>(result);
        } 
        
        [Fact]
        public async Task Update_ReturnsConflict_WhenCategoryNameAlreadyExists()
        {
            // Arrange
            var category = categories.First();
            CategoryUpdateDto dto = new CategoryUpdateDto() {Name = categories.Skip(1).First().Name};
            var target = new CategoriesController(context);

            // Act
            var result = await target.Update(category.Id, dto);
            
            // Assert
            var objectResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal($"Category with name {dto.Name} already exists.", objectResult.Value);
        } 
        
        [Fact]
        public async Task Update_ReturnsNotFound_WhenCategoryDoesNotExist()
        {
            // Arrange
            CategoryUpdateDto dto = new CategoryUpdateDto();
            var target = new CategoriesController(context);

            // Act
            var result = await target.Update(NonExistingId, dto);
            
            // Assert
            Assert.IsType<NotFoundResult>(result);
        } 
        
        [Fact]
        public async Task Delete_DeletesCategory_WhenSucceeded()
        {
            // Arrange
            var category = categories.First();
            var target = new CategoriesController(context);

            // Act
            var result = await target.Delete(category.Id);
            
            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.DoesNotContain(context.Categories, c => c.Id == category.Id);
        } 
        
        [Fact]
        public async Task Delete_ReturnNotFound_WhenCategoryDoesNotExist()
        {
            // Arrange
            var target = new CategoriesController(context);

            // Act
            var result = await target.Delete(NonExistingId);
            
            // Assert
            Assert.IsType<NotFoundResult>(result);
        } 
    }
}