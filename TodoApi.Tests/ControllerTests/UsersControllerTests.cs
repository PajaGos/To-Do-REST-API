using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Controllers;
using TodoApi.Data;
using TodoApi.DTOs.Tasks;
using TodoApi.DTOs.Users;
using TodoApi.Mappers;
using TodoApi.Models;
using Xunit;

namespace TodoApi.Tests;

public class UsersControllerTests : IAsyncLifetime
{
    private const int NonExistingId = 150;
    private ToDoContext context;
    private List<User> users;
    
    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ToDoContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // InMemory provider, unique db for each test
            .Options;        
        
        users = new List<User>
        {
            new User { Id = 1, UserName = "User1", Email = "user1@email.com", CreatedAt = DateTime.MinValue },
            new User { Id = 2, UserName = "User2", Email = "user2@email.com", CreatedAt = DateTime.MinValue },
            new User { Id = 3, UserName = "User3", Email = "user3email.com", CreatedAt = DateTime.MinValue }
        };
        
        context = new ToDoContext(options);
        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAll_ReturnsAllAvailableUsers_WhenUsersArePresent()
    {
        // Arrange
        var target = new UsersController(context);
        
        // Act
        var result = await target.GetAll();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<UserDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<List<UserDto>>(okResult.Value);
        Assert.Equal(users.Count, returnValue.Count);
    }
    
    [Fact]
    public async Task GetById_ReturnsExpectedUser_WhenUserExists()
    {
        // Arrange
        int id = 1;
        User expectedUser = users.First(t => t.Id == id);
        UserDto expectedUserDto = new UserDto()
        {
            Email = expectedUser.Email,
            UserName = expectedUser.UserName
        };
        
        var target = new UsersController(context);

        // Act
        var result = await target.GetById(id);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equivalent(expectedUserDto, returnValue);
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
    public async Task Create_ReturnsUnmodifiedCreatedUser_WhenUserIsCreated()
    {
        // Arrange
        string userName = "User150";
        string email = "user150@email.com";
        var user = new UserCreateDto() { UserName = userName, Email = email};
        var expectedUser = new UserDto() { UserName = userName, Email = email };
        var target = new UsersController(context);

        // Act
        var result = await target.Create(user);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserDto>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var returnValue = Assert.IsType<UserDto>(createdAtActionResult.Value);
        Assert.Equivalent(expectedUser, returnValue);
    }
    
    [Fact]
    public async Task Create_ReturnsConflict_WhenUserNameAlradyExists()
    {
        // Arrange
        var user = new UserCreateDto() { UserName = users.First().UserName, Email = "newEmail@email.com"};
        var target = new UsersController(context);

        // Act
        var result = await target.Create(user);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserDto>>(result);
        var conflictResult = Assert.IsType<ConflictObjectResult>(actionResult.Result);
        Assert.Equal((int)HttpStatusCode.Conflict, conflictResult.StatusCode);
        Assert.Equal($"Username {user.UserName} already exists.", conflictResult.Value);
    }
    
    [Fact]
    public async Task Create_ReturnsConflict_WhenUserEmailAlradyExists()
    {
        // Arrange
        var user = new UserCreateDto() { UserName = "NewUserName", Email = users.First().Email};
        var target = new UsersController(context);

        // Act
        var result = await target.Create(user);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserDto>>(result);
        var conflictResult = Assert.IsType<ConflictObjectResult>(actionResult.Result);
        Assert.Equal($"Username with the same email {user.Email} already exists.", conflictResult.Value);
    }
    
        
    [Fact]
    public async Task Update_ReturnsNoContent_WhenUserIsUpdated()
    {
        // Arrange
        User originalUser = users.First();
        var updatedUser = new UserUpdateDto()
        {
            UserName = "NonMatchingId",
            Email = "",
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
        var updatedUser = new UserUpdateDto()
        {
            UserName = "NonMatchingId",
            Email = "",
        };
        
        var expectedUser = new UserDto()
        {
            UserName = updatedUser.UserName,
            Email = updatedUser.Email,
        };
        var target = new UsersController(context);

        // Act
        var result = await target.Update(originalUser.Id, updatedUser);
        var updatedItemFromDatabase = await context.Users.FindAsync(originalUser.Id);
        
        // Assert
        Assert.Equivalent(expectedUser, updatedItemFromDatabase);
    }
    
    [Fact]
    public async Task Update_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var updatedUser = new UserUpdateDto()
        {
            UserName = "NonMatchingId",
            Email = "",
        };
        var target = new UsersController(context);

        // Act
        var result = await target.Update(NonExistingId, updatedUser);

        // Assert
        Assert.IsType<NotFoundResult>(result);
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
        Assert.DoesNotContain(context.Users, u => u.Id == originalUser.Id);
    }
        
    [Fact]
    public async Task Delete_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var target = new UsersController(context);

        // Act
        var result = await target.Delete(NonExistingId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    
    [Fact]
    public async Task GetTasksForUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var target = new UsersController(context);

        // Act
        var result = await target.GetTasksForUser(NonExistingId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<TaskItemDto>>>(result);
        Assert.IsType<NotFoundResult>(actionResult.Result);
    }
    
    [Fact]
    public async Task GetTasksForUser_ReturnsTasks_WhenUserExists()
    {
        // Arrange
        User user = users.First();
        var target = new UsersController(context);
        IEnumerable<TaskItemDto> expectedTasks = await AddTasksToUser(user);
            
        // Act
        var result = await target.GetTasksForUser(user.Id);

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<TaskItemDto>>>(result);
        var objectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<TaskItemDto>>(objectResult.Value);
        Assert.Equivalent(expectedTasks, returnValue);
    }

    private async Task<IEnumerable<TaskItemDto>> AddTasksToUser(User user)
    {
        user.Tasks.Add(new TaskItem { Title = "Task 1", IsCompleted = false });
        user.Tasks.Add(new TaskItem { Title = "Task 2", IsCompleted = true });
        await context.SaveChangesAsync();
        
        return user.Tasks.Select(t => t.ToDto());
    }
    
    public Task DisposeAsync()
    {
        context?.Dispose();
        return Task.CompletedTask;
    }
}
