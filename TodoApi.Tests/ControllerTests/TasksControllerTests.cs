using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Controllers;
using TodoApi.Data;
using TodoApi.DTOs.Common;
using TodoApi.DTOs.Tasks;
using TodoApi.Models;
using Xunit;

namespace TodoApi.Tests;

public class TasksControllerTests : IAsyncLifetime
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
            new User { Id = 1, UserName = "TestUser1" , Email = "user1@email.com"},
            new User { Id = 2, UserName = "TestUser2" , Email = "user2@email.com"},
        };
        
        var testUser1 = users.First();
        var testUser2 = users.Skip(1).First();
    
        tasks = new List<TaskItem>
        {
            new TaskItem { Id = 1, Title = "Test1", IsCompleted = false, DueDate = DateTime.MaxValue, User = testUser1, UserId = testUser1.Id},
            new TaskItem { Id = 2, Title = "Test2", IsCompleted = true, DueDate = DateTime.MaxValue, User = testUser1, UserId = testUser1.Id},
            new TaskItem { Id = 3, Title = "Test3", IsCompleted = false, DueDate = DateTime.MaxValue, User = testUser1, UserId = testUser1.Id},
            
            new TaskItem { Id = 4, Title = "Test4", IsCompleted = false, DueDate = DateTime.MaxValue, User = testUser2, UserId = testUser2.Id},
            new TaskItem { Id = 5, Title = "Test5", IsCompleted = true, DueDate = DateTime.MaxValue, User = testUser2, UserId = testUser2.Id},
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

    [Fact]
    public async Task GetAll_ReturnsAllAvailableTasks_WhenTasksArePresent()
    {
        // Arrange
        var target = new TasksController(context);
        
        // Act
        var result = await target.Get(null, null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PagedResult<TaskItemDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<PagedResult<TaskItemDto>>(okResult.Value);
        Assert.Equal(tasks.Count, returnValue.Items.Count());
    }
    
    [Fact]
    public async Task GetAll_ReturnsAllUserTasks_WhenUserIsSpecified()
    {
        // Arrange
        var userId = users.First().Id;
        var userTasks = tasks.Where(t => t.UserId == userId).ToList();
        var taskCategory = taskCategories.First(x => x.TaskId == userTasks.First().Id);
        var category = categories.First(x => x.Id == taskCategory.CategoryId);
        var filteredUserTasks = userTasks.Where(t => t.TaskCategories.Any(x => x.CategoryId == category.Id)).ToList();
        var target = new TasksController(context);
        
        // Act
        var result = await target.Get(userId, category.Name);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PagedResult<TaskItemDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<PagedResult<TaskItemDto>>(okResult.Value);
        Assert.Equal(filteredUserTasks.Count, returnValue.Items.Count());
    }
    
    [Fact]
    public async Task GetAll_ReturnsAllUserTasksInCategory_WhenUserAndTaskCategoryIsSpecified()
    {
        // Arrange
        var userId = users.First().Id;
        var userTasks = tasks.Where(t => t.UserId == userId).ToList();
        var target = new TasksController(context);
        
        // Act
        var result = await target.Get(userId, null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PagedResult<TaskItemDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<PagedResult<TaskItemDto>>(okResult.Value);
        Assert.Equal(userTasks.Count, returnValue.Items.Count());
    }
    
    [Theory]
    [InlineData("title", "asc")]
    [InlineData("title", "desc")]
    public async Task GetAll_ReturnsSortedTasksByTitle_WhenSortByTitleIsSpecified(string sortBy, string sortOrder)
    {
        // Arrange
        var userId = users.First().Id;
        var userTasks = tasks.Where(t => t.UserId == userId).ToList();
        userTasks = sortOrder == "asc" 
            ? userTasks.OrderBy(x => x.Title).ToList() 
            : userTasks.OrderByDescending(x => x.Title).ToList();
        
        var target = new TasksController(context);
        
        // Act
        var result = await target.Get(userId, null, sortBy: sortBy, sortOrder: sortOrder);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PagedResult<TaskItemDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<PagedResult<TaskItemDto>>(okResult.Value);
        Assert.Equivalent(userTasks.Select(x => x.Title), returnValue.Items.Select(x => x.Title));
    }
    
    [Theory]
    [InlineData("duedate", "asc")]
    [InlineData("duedate", "desc")]
    public async Task GetAll_ReturnsSortedTasksByDueDate_WhenSortByDueDateIsSpecified(string sortBy, string sortOrder)
    {
        // Arrange
        var userId = users.First().Id;
        var userTasks = tasks.Where(t => t.UserId == userId).ToList();
        userTasks = sortOrder == "asc" 
            ? userTasks.OrderBy(x => x.DueDate).ToList() 
            : userTasks.OrderByDescending(x => x.DueDate).ToList();
        
        var target = new TasksController(context);
        
        // Act
        var result = await target.Get(userId, null, sortBy: sortBy, sortOrder: sortOrder);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PagedResult<TaskItemDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<PagedResult<TaskItemDto>>(okResult.Value);
        Assert.Equivalent(userTasks.Select(x => x.DueDate), returnValue.Items.Select(x => x.DueDate));
    }
    
    [Theory]
    [InlineData("priority", "asc")]
    [InlineData("priority", "desc")]
    public async Task GetAll_ReturnsSortedTasksByPriority_WhenSortPriorityIsSpecified(string sortBy, string sortOrder)
    {
        // Arrange
        var userId = users.First().Id;
        var userTasks = tasks.Where(t => t.UserId == userId).ToList();
        userTasks = sortOrder == "asc" 
            ? userTasks.OrderBy(x => x.Priority).ToList() 
            : userTasks.OrderByDescending(x => x.Priority).ToList();
        
        var target = new TasksController(context);
        
        // Act
        var result = await target.Get(userId, null, sortBy: sortBy, sortOrder: sortOrder);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PagedResult<TaskItemDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<PagedResult<TaskItemDto>>(okResult.Value);
        Assert.Equivalent(userTasks.Select(x => x.Priority), returnValue.Items.Select(x => x.Priority));
    }
    
    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 2)]
    [InlineData(2, 3)]
    public async Task GetAll_ReturnsPagedTasks_WhenPagingIsDefined(int pageNumber, int pageSize)
    {
        // Arrange
        int skipCount = pageSize * (pageNumber - 1); 
        var userId = users.First().Id;
        var userTasks = tasks.Where(t => t.UserId == userId).Skip(skipCount).Take(pageSize).ToList();
        
        var target = new TasksController(context);
        
        // Act
        var result = await target.Get(userId, null, pageNumber: pageNumber, pageSize: pageSize);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PagedResult<TaskItemDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<PagedResult<TaskItemDto>>(okResult.Value);
        Assert.Equivalent(userTasks.Select(x => x.Title), returnValue.Items.Select(x => x.Title));
    }
    
    [Fact]
    public async Task GetById_ReturnsExpectedTask_WhenTaskExists()
    {
        // Arrange
        int id = 1;
        TaskItem expectedTask = tasks.First(t => t.Id == id); 
        var target = new TasksController(context);

        // Act
        var result = await target.GetById(id);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TaskItemDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<TaskItemDto>(okResult.Value);
        Assert.Equal(expectedTask.Title, returnValue.Title);
    }
        
    [Fact]
    public async Task GetById_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        int id = tasks.Count + 1;
        var target = new TasksController(context);

        // Act
        var result = await target.GetById(id);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
    
    [Fact]
    public async Task Create_ReturnsUnmodifiedCreatedTask_WhenTaskIsCreated()
    {
        // Arrange
        string title = "Update Readme";
        bool done = false;
        var expectedTaskItemState = new TaskItemDto() { Title = title, IsCompleted = done};
        var target = new TasksController(context);

        // Act
        var result = await target.Create(new TaskItemCreateDto()
        {
            Title = expectedTaskItemState.Title, 
            IsCompleted = expectedTaskItemState.IsCompleted, 
            UserId = users.First().Id,
        });

        // Assert
        var actionResult = Assert.IsType<ActionResult<TaskItemDto>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var returnValue = Assert.IsType<TaskItemDto>(createdAtActionResult.Value);
        
        Assert.Equal(expectedTaskItemState.Title, returnValue.Title);
        Assert.Equal(expectedTaskItemState.IsCompleted, returnValue.IsCompleted);
    }
    
    [Fact]
    public async Task Create_ReturnsTaskWithLinkedCategory_WhenTaskCategoryIsProvided()
    {
        // Arrange
        string title = "Update Readme";
        bool done = false;
        var target = new TasksController(context);
        var category = categories.First();
        var task = new TaskItemCreateDto()
        {
            Title = title,
            IsCompleted = done,
            UserId = users.First().Id,
            CategoryIds = new List<int>() { category.Id, }
        };
        
        // Act
        var result = await target.Create(task);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TaskItemDto>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var returnValue = Assert.IsType<TaskItemDto>(createdAtActionResult.Value);
        
        Assert.Equal(task.CategoryIds.Count, returnValue.Categories.Count);
        Assert.Equal(category.Name, returnValue.Categories.First().Name);
    }
    
    [Fact]
    public async Task Create_ReturnsBadRequest_WhenTaskCategoryDoesNotExist()
    {
        // Arrange
        string title = "Update Readme";
        bool done = false;
        var target = new TasksController(context);
        var task = new TaskItemCreateDto()
        {
            Title = title,
            IsCompleted = done,
            UserId = users.First().Id,
            CategoryIds = new List<int>() { NonExistingId },
        };
        
        // Act
        var result = await target.Create(task);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TaskItemDto>>(result);
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }
    
    [Fact]
    public async Task Create_ReturnsBadRequest_WhenUserDoesNotExist()
    {
        // Arrange
        var task = new TaskItemCreateDto()
        {
            Title = "Some Task", 
            IsCompleted = false, 
            UserId = NonExistingId, 
        };
        
        var target = new TasksController(context);

        // Act
        var result = await target.Create(task);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TaskItemDto>>(result);
        var requestObjectResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal($"User with ID {task.UserId} does not exist.", requestObjectResult.Value);
    }
        
    [Fact]
    public async Task Update_ReturnsNoContent_WhenTaskIsUpdated()
    {
        // Arrange
        TaskItem originalTaskItem = tasks.First();
        var updatedItem = new TaskItemUpdateDto()
        {
            Title = originalTaskItem.Title, 
            IsCompleted = !originalTaskItem.IsCompleted,
            UserId = users.First().Id,
        };
        var target = new TasksController(context);

        // Act
        var result = await target.Update(originalTaskItem.Id, updatedItem);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
    
    [Fact]
    public async Task Update_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var updatedItem = new TaskItemUpdateDto()
        {
            Title = "NonExisting", 
            IsCompleted = false
        };
        var target = new TasksController(context);

        // Act
        var result = await target.Update(150, updatedItem);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    
    [Fact]
    public async Task Update_ReturnsBadRequest_WhenUserInTaskDoesNotExist()
    {
        // Arrange
        TaskItem originalTaskItem = tasks.First();
        var updatedItem = new TaskItemUpdateDto()
        {
            Title = originalTaskItem.Title, 
            IsCompleted = !originalTaskItem.IsCompleted,
            UserId = 150 // Non-existing user
        };
        var target = new TasksController(context);

        // Act
        var result = await target.Update(originalTaskItem.Id, updatedItem);

        // Assert
        var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal($"User with ID {updatedItem.UserId} does not exist.", badRequestObjectResult.Value);
    }
        
    [Fact]
    public async Task Delete_RemovesTaskAndReturnsNoContent_WhenItemExists()
    {
        // Arrange
        TaskItem originalTaskItem = tasks.First();
        var target = new TasksController(context);

        // Act
        var result = await target.Delete(originalTaskItem.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.DoesNotContain(context.Tasks, t => t.Id == originalTaskItem.Id);
    }
        
    [Fact]
    public async Task Delete_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        int notExistingId = 150;
        var target = new TasksController(context);

        // Act
        var result = await target.Delete(notExistingId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    
    public Task DisposeAsync()
    {
        context?.Dispose();
        return Task.CompletedTask;
    }
}
