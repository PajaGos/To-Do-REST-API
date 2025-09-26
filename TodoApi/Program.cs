using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TodoApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add SQLite database
builder.Services.AddDbContext<ToDoContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers + swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// generate OpenAPI specification
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TodoApi",
        Version = "v1",
        Description = "Simple REST API for managing a to-do list",
    });
});

var app = builder.Build();

// Ensure database is created and apply any pending migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ToDoContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // add middleware to provide generated OpenAPI specification as a JSON endpoint
    app.UseSwaggerUI(); // add Swagger UI
}

// map controller to be able to route HTTP requests to proper controller
app.MapControllers();

app.Run();