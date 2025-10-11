using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Controllers;
using TodoApi.Data;
using TodoApi.Models;
using Xunit;

namespace TodoApi.Tests;

public class TasksControllerTests : IAsyncLifetime
{
    private ToDoContext context;

    [Fact]
    public async Task GetAll_ReturnsAllAvailableTasks_WhenTasksArePresent()
    {
        // Arrange
        var target = new TasksController(context);
        
        // Act
        var result = await target.GetAll();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<TaskItem>>>(result);
        var returnValue = Assert.IsType<List<TaskItem>>(actionResult.Value);
        Assert.Equal(tasks.Count, returnValue.Count);
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
        var actionResult = Assert.IsType<ActionResult<TaskItem>>(result);
        var returnValue = Assert.IsType<TaskItem>(actionResult.Value);
        Assert.Equal(expectedTask, returnValue);
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
    public async Task Create_ReturnsCreatedTask_WhenTaskIsCreated()
    {
        // Arrange
        var taskItem = new TaskItem() { Id = 150, Title = "Update Readme", IsCompleted = false };
        var target = new TasksController(context);

        // Act
        var result = await target.Create(taskItem);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TaskItem>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var returnValue = Assert.IsType<TaskItem>(createdAtActionResult.Value);
        Assert.Equal(taskItem, returnValue);
    }
    
    [Fact]
    public async Task Create_ReturnsUnmodifiedCreatedTask_WhenTaskIsCreated()
    {
        // Arrange
        int id = 150;
        string title = "Update Readme";
        bool done = false;
        var expectedTaskItemState = new TaskItem() { Id = id, Title = title, IsCompleted = done };
        var target = new TasksController(context);

        // Act
        var result = await target.Create(new TaskItem() { Id = id, Title = title, IsCompleted = done });

        // Assert
        var actionResult = Assert.IsType<ActionResult<TaskItem>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var returnValue = Assert.IsType<TaskItem>(createdAtActionResult.Value);
        Assert.Equivalent(expectedTaskItemState, returnValue);
    }
    
        
    [Fact]
    public async Task Update_ReturnsNoContent_WhenTaskIsUpdated()
    {
        // Arrange
        TaskItem originalTaskItem = tasks.First();
        var updatedItem = new TaskItem()
        {
            Id = originalTaskItem.Id, 
            Title = originalTaskItem.Title, 
            IsCompleted = !originalTaskItem.IsCompleted
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
        var updatedItem = new TaskItem()
        {
            Id = 150, 
            Title = "NonExisting", 
            IsCompleted = false
        };
        var target = new TasksController(context);

        // Act
        var result = await target.Update(updatedItem.Id, updatedItem);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    
        
    [Fact]
    public async Task Update_ReturnsBadRequest_WhenIdDoesNotMatchUserId()
    {
        // Arrange
        TaskItem originalTaskItem = tasks.First();
        var updatedItem = new TaskItem()
        {
            Id = originalTaskItem.Id + 1, 
            Title = originalTaskItem.Title, 
            IsCompleted = !originalTaskItem.IsCompleted
        };
        
        var target = new TasksController(context);

        // Act
        var result = await target.Update(originalTaskItem.Id, updatedItem);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }
    
        
    [Fact]
    public async Task Delete_RemovesUserAndReturnsNoContent_WhenItemExists()
    {
        // Arrange
        TaskItem originalTaskItem = tasks.First();
        var target = new TasksController(context);

        // Act
        var result = await target.Delete(originalTaskItem.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);
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
    
    private readonly List<TaskItem> tasks = new List<TaskItem>
    {
        new TaskItem { Id = 1, Title = "Test1", IsCompleted = false },
        new TaskItem { Id = 2, Title = "Test2", IsCompleted = true },
        new TaskItem { Id = 3, Title = "Test3", IsCompleted = false }
    };
    
    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ToDoContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // InMemory provider, unique db for each test
            .Options;        
        
        context = new ToDoContext(options);
        context.Tasks.AddRange(tasks);
        await context.SaveChangesAsync();
    }
    
    public Task DisposeAsync()
    {
        context?.Dispose();
        return Task.CompletedTask;
    }
}
