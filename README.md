# TodoApi

A simple REST API for managing TODO tasks, built with ASP.NET Core, EF Core, and Docker.

## Table of Contents

* [Features](#features)
* [Project Structure](#project-structure)
* [Prerequisites](#prerequisites)
* [Getting Started](#getting-started)
* [Running with Docker](#running-with-docker)
* [API Documentation](#api-documentation)
* [Database Migrations](#database-migrations)
* [Contributing](#contributing)
* [License](#license)

---

## Features

* REST API for CRUD operations on TODO tasks
* SQLite database with EF Core migrations
* Swagger UI for easy API exploration
* Dockerized for easy development and deployment
* Ready for team collaboration with versioned migrations

---

## Project Structure

```
To-Do-REST-API/
│
├─ .gitignore
├─ README.md
├─ TODO.md
├─ docker-compose.yml
├─ Dockerfile
├─ TodoApi.sln
├─ data/                # runtime SQLite DB (not committed)
└─ TodoApi/
    ├─ Controllers/
    ├─ Data/
    │   └─ Configurations/
    │   └─ Migrations/
    ├─ Dtos/
    │   └─ Category/
    │   └─ Common/
    │   └─ TaskCategory/
    │   └─ Tasks/
    │   └─ Users/
    ├─ Mappers/
    ├─ Models/
    ├─ Program.cs
    ├─ appsettings.json
    ├─ appsettings.Development.json
    └─ TodoApi.csproj
└─ TodoApi.Tests/
    ├─ ControllerTests/
    └─ DtoValidationTests
```

---

## Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* [Docker](https://www.docker.com/get-started) (optional, for containerized setup)

---

## Getting Started (Local Development)

1. Clone the repository:

```bash
git clone https://github.com/yourusername/To-Do-REST-API.git
cd To-Do-REST-API
```

2. Restore dependencies and build the project:

```bash
cd TodoApi
dotnet restore
dotnet build
```

3. Run the API:

```bash
dotnet run
```

* The API will be available at `http://localhost:5050`
* Swagger UI: `http://localhost:5050/swagger/index.html`

---

## Running with Docker

1. Build and start the containers:

```bash
docker compose up --build
```

2. API will be available at `http://localhost:5050`
3. SQLite database will be created automatically in `./data/todo.db`

> Make sure `data/` folder exists or Docker will create it automatically.

---

## API Documentation

* Swagger UI is available at `/swagger/index.html`
* Tasks Endpoints:

| Method | Endpoint                                                                           | Description                                                         |
| ------ |------------------------------------------------------------------------------------|---------------------------------------------------------------------|
| GET    | tasks?userId=1&category=Work&pageNumber=1&pageSize=10&sortBy=dueDate&sortOrder=asc | Filter tasks by user/category, Sort tasks by Title/Priority/Duedate |
| GET    | /tasks/{id}                                                                        | Get task by ID                                                      |
| POST   | /tasks/                                                                            | Create new task                                                     |
| PUT    | /tasks/{id}                                                                        | Update existing task                                                |
| DELETE | /tasks/{id}                                                                        | Delete task                                                         |

* User Endpoints:

| Method | Endpoint          | Description              |
|--------|-------------------|--------------------------|
| GET    | /users            | Get all users            |
| GET    | /users/{id}       | Get user by ID           |
| GET    | /users/{id}/tasks | get all tasks for a user |
| POST   | /users/           | Create new user          |
| PUT    | /users/{id}       | Update existing user     |
| DELETE | /users/{id}       | Delete user              |

* Category Endpoints:

| Method | Endpoint               | Description                 |
|--------|------------------------|-----------------------------|
| GET    | /categories            | Get all categories          |
| GET    | /categories/{id}       | Get category by ID          |
| GET    | /categories/{id}/tasks | Get all tasks in a category |
| POST   | /categories/           | Create new category         |
| PUT    | /categories/{id}       | Update existing category    |
| DELETE | /categories/{id}       | Delete category             |

* Task Category Endpoints

| Method | Endpoint                     | Description                         |
|--------|------------------------------|-------------------------------------|
| GET    | /tasks/{id}/categories       | Get all categories assigned to task |
| POST   | /tasks/{id}/categories/{id}  | Assign category to existing task    |
| DELETE | /tasks/{id}/categories/{id}  | Remove category from existing task  |

---

## Database Migrations

1. Add a migration:

```bash
dotnet ef migrations add MigrationName --output-dir Data/Migrations
```

2. Migrations are applied automatically on application startup (code in `Program.cs`):

```csharp
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<TodoContext>();
db.Database.Migrate();
```

> Migrations are versioned and should be committed to the repository.

---

## Contributing

* Fork the repository
* Create a feature branch with your initials: `git checkout -b pgo/my-feature`
* Commit changes: `git commit -m "feat: Add my feature"`
* Push to branch: `git push origin pgo/my-feature`
* Open a Pull Request

---