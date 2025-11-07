# TODO List for TODO API Project

This document tracks the planned tasks and enhancements for the TODO API project.

---

## 1️⃣ Project Setup & Core

- [x] Create solution and API project (`ToDoApi.sln`)
- [x] Configure `TodoContext` and SQLite database
- [x] Create entity models:
    - `User`
    - `TaskItem`
    - `Category`
    - `TaskCategory` (explicit join table)
- [x] Add DTO classes for all entities:
    - `UserDto`
    - `TaskItemDto`
    - `CategoryDto`
- [x] Add EF Core configurations (`Configurations/` folder)
- [x] Apply initial EF Core migration (`InitialCreate`)
- [x] Dockerize the project:
    - `Dockerfile`
    - `docker-compose.yml`

---

## 2️⃣ API Endpoints

### Users
- [x] GET `/users` – list all users
- [x] POST `/users` – create a new user
- [x] GET `/users/{id}` – get user by ID
- [x] PUT `/users/{id}` – update user
- [x] DELETE `/users/{id}` – delete user
- [x] GET `/users/{id}/tasks` – get all tasks for a user

### Categories
- [x] GET `/categories` – list all categories
- [x] POST `/categories` – create a new category
- [x] GET `/categories/{id}` – get category by ID
- [x] PUT `/categories/{id}` – update category
- [x] DELETE `/categories/{id}` – detelete category
- [x] GET `/categories/{id}/tasks` – get all tasks in a category

### Task Categories
- [x] GET `/tasks/{id}/categories` – list categories for a task
- [x] POST `/tasks/{id}/categories/{id}` – assign category to task
- [x] DELETE `/tasks/{id}/categories/{id}` – remove category from task

### Tasks
- [x] GET `/tasks?userId=&category=` – filter tasks by user and/or category
- [x] GET `/tasks/{id}` – get task by ID
- [x] POST `/tasks` – create task
- [x] PUT `/tasks/{id}` – update task
- [x] DELETE `/tasks/{id}` – delete task

---

## 3️⃣ Enhancements

- [x] Add paging & sorting for GET `/tasks`
- [ ] Add search by title or due date
- [ ] Add priority filter for tasks
- [x] Add DTO validation (e.g., title required, due date in future)
- [x] Enable automatic EF Core migrations on app startup

---

## 4️⃣ Testing / DevOps

- [x] Unit tests for controllers (mock DbContext or InMemory DB)
- [x] Integration tests with SQLite in-memory DB
- [x] Setup Swagger for API documentation
- [x] Add detailed README with example requests/responses

---

## 5️⃣ Optional / Advanced Features

- [ ] Authentication & Authorization (tasks per user)
- [ ] Soft delete tasks instead of hard delete
- [ ] Add `CompletedAt` timestamp for completed tasks
- [ ] Activity log / audit trail for TaskCategory assignments

---

## Notes

- Keep `DTOs` and `Models` separate for clarity and safety
- Use explicit join table `TaskCategory` for task-category relationships
- Project should remain modular: Controllers / DTOs / Models / Data / Configurations
