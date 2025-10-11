using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Controllers;
using TodoApi.Data;
using TodoApi.Models;
using Xunit;

namespace TodoApi.Tests;

public class UsersControllerTests : IAsyncLifetime
{
    private ToDoContext context;

    [Fact]
    public async Task GetAll_ReturnsAllAvailableUsers_WhenUsersArePresent()
    {
        // Arrange
        var target = new UsersController(context);
        
        // Act
        var result = await target.GetAll();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<User>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<List<User>>(okResult.Value);
        Assert.Equal(users.Count, returnValue.Count);
    }
    
    [Fact]
    public async Task GetById_ReturnsExpectedUser_WhenUserExists()
    {
        // Arrange
        int id = 1;
        User expectedTask = users.First(t => t.Id == id); 
        var target = new UsersController(context);

        // Act
        var result = await target.GetById(id);

        // Assert
        var actionResult = Assert.IsType<ActionResult<User>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<User>(okResult.Value);
        Assert.Equal(expectedTask, returnValue);
    }
    
        
    [Fact]
    public async Task GetById_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        int id = users.Count + 1;
        var target = new UsersController(context);

        // Act
        var result = await target.GetById(id);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
        
    [Fact]
    public async Task Create_ReturnsCreatedUser_WhenUserIsCreated()
    {
        // Arrange
        int id = 150;
        string userName = "User150";
        string email = "user150@email.com";
        DateTime createdAt = DateTime.MinValue;
        var expectedUser = new User() { Id = id, UserName = userName, Email = email, CreatedAt = createdAt };
        var target = new UsersController(context);

        // Act
        var result = await target.Create(expectedUser);

        // Assert
        var actionResult = Assert.IsType<ActionResult<User>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var returnValue = Assert.IsType<User>(createdAtActionResult.Value);
        Assert.Equal(expectedUser, returnValue);
    }
    
    [Fact]
    public async Task Create_ReturnsUnmodifiedCreatedUser_WhenUserIsCreated()
    {
        // Arrange
        int id = 150;
        string userName = "User150";
        string email = "user150@email.com";
        DateTime createdAt = DateTime.MinValue;
        var expectedUserState = new User() { Id = id, UserName = userName, Email = email, CreatedAt = createdAt };
        var target = new UsersController(context);

        // Act
        var result = await target.Create(new User() { Id = id, UserName = userName, Email = email, CreatedAt = createdAt });

        // Assert
        var actionResult = Assert.IsType<ActionResult<User>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var returnValue = Assert.IsType<User>(createdAtActionResult.Value);
        Assert.Equivalent(expectedUserState, returnValue);
    }
    
        
    [Fact]
    public async Task Update_ReturnsNoContent_WhenUserIsUpdated()
    {
        // Arrange
        User originalUser = users.First();
        var updatedUser = new User()
        {
            Id = originalUser.Id, 
            UserName = "NonMatchingId",
            Email = "",
            CreatedAt = DateTime.MinValue
        };
        var target = new UsersController(context);

        // Act
        var result = await target.Update(originalUser.Id, updatedUser);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
    
    [Fact]
    public async Task Update_UpdatesItemInDatabase_WhenUserIsUpdated()
    {
        // Arrange
        User originalUser = users.First();
        var updatedUser = new User()
        {
            Id = originalUser.Id, 
            UserName = "NonMatchingId",
            Email = "",
            CreatedAt = DateTime.MinValue
        };
        var target = new UsersController(context);

        // Act
        var result = await target.Update(originalUser.Id, updatedUser);
        var updatedItemFromDatabase = await context.Users.FindAsync(originalUser.Id);
        
        // Assert
        Assert.Equivalent(updatedUser, updatedItemFromDatabase);
    }
    
    [Fact]
    public async Task Update_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var updatedUser = new User()
        {
            Id = 150, 
            UserName = "NonMatchingId",
            Email = "",
            CreatedAt = DateTime.MinValue
        };
        var target = new UsersController(context);

        // Act
        var result = await target.Update(updatedUser.Id, updatedUser);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    
        
    [Fact]
    public async Task Update_ReturnsBadRequest_WhenIdDoesNotMatchUserId()
    {
        // Arrange
        User originalUser = users.First();
        var updatedUser = new User()
        {
            Id = originalUser.Id + 1, 
            UserName = "NonMatchingId",
            Email = "",
            CreatedAt = DateTime.MinValue
        };
        
        var target = new UsersController(context);

        // Act
        var result = await target.Update(originalUser.Id, updatedUser);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }
    
        
    [Fact]
    public async Task Delete_RemovesUserAndReturnsNoContent_WhenUserExists()
    {
        // Arrange
        User originalUser = users.First();
        var target = new UsersController(context);

        // Act
        var result = await target.Delete(originalUser.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
        
    [Fact]
    public async Task Delete_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        int notExistingId = 150;
        var target = new UsersController(context);

        // Act
        var result = await target.Delete(notExistingId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    
    private readonly List<User> users = new List<User>
    {
        new User { Id = 1, UserName = "User1", Email = "user1@email.com", CreatedAt = DateTime.MinValue },
        new User { Id = 2, UserName = "User2", Email = "user2@email.com", CreatedAt = DateTime.MinValue },
        new User { Id = 3, UserName = "User3", Email = "user3email.com", CreatedAt = DateTime.MinValue }
    };
    
    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ToDoContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // InMemory provider, unique db for each test
            .Options;        
        
        context = new ToDoContext(options);
        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }
    
    public Task DisposeAsync()
    {
        context?.Dispose();
        return Task.CompletedTask;
    }
}
