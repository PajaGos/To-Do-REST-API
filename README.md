---

# TodoApi

> A simple REST API for managing TODO tasks, built with ASP.NET Core, EF Core, and Docker.

---

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
├─ docker-compose.yml
├─ Dockerfile
├─ TodoApi.sln
├─ data/                # runtime SQLite DB (not committed)
└─ TodoApi/
    ├─ Controllers/
    ├─ Models/
    ├─ Data/
    │   └─ Migrations/
    ├─ Program.cs
    ├─ appsettings.json
    ├─ appsettings.Development.json
    └─ TodoApi.csproj
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
* Endpoints:

| Method | Endpoint       | Description          |
| ------ | -------------- | -------------------- |
| GET    | /api/todo      | Get all tasks        |
| GET    | /api/todo/{id} | Get task by ID       |
| POST   | /api/todo      | Create new task      |
| PUT    | /api/todo/{id} | Update existing task |
| DELETE | /api/todo/{id} | Delete task          |

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