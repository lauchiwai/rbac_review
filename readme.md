
# Permission + Review Flow System

This is an ASP.NET Core-based permission management and multi-level review flow system. The system adopts Role-Based Access Control (RBAC) design, supports multi-level review workflows, and ensures the security and traceability of business processes.

## Features

- ✅ Role-Based Access Control (RBAC)
- 🔒 Multi-level Review Flow Management
- 📦 Complete CRUD Operations
- 🚀 RESTful API Design
- ⚡ Permission Verification and Access Control
- 🎯 Review History Tracking

## Technology Stack

- **Backend Framework**: ASP.NET Core
- **Database**: Entity Framework Core
- **Language**: C#
- **Architecture**: Layered Architecture (Controller-Service-Repository)
- **Authentication**: Custom Permission Verification Mechanism

## Quick Start

### System Requirements

- .NET 8.0
- Supported Database (SQL Server/MySQL/PostgreSQL)
- Local File System Write Permissions

### Installation Steps

1. **Clone Project**

git clone https://github.com/lauchiwai/rbac_review.git
cd rbac_review

2. **Restore NuGet Packages**

dotnet restore

3. **Configure Database Connection**

Update connection string in `appsettings.json`:

{
  "ConnectionStrings": {
    "DefaultConnection": "Your_Connection_String"
  }
}

4. **Run Database Migrations**

dotnet ef migrations add InitialCreate --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

dotnet ef database update --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

5. **Run Application**

dotnet run

## API Documentation

### Permission Management API

| Method | Endpoint                         | Description           | Request Example                                |
| ------ | -------------------------------- | --------------------- | ---------------------------------------------- |
| GET    | `/api/permissions/get-all`     | Get all permissions   | -                                              |
| GET    | `/api/permissions/get/{id}`    | Get permission by ID  | `/api/permissions/get/1`                     |
| POST   | `/api/permissions/create`      | Create new permission | `{"permissionName": "todo_create"}`          |
| PUT    | `/api/permissions/update/{id}` | Update permission     | `{"id": 1, "permissionName": "todo_create"}` |
| DELETE | `/api/permissions/delete/{id}` | Delete permission     | `/api/permissions/delete/1`                  |

### Role Management API

| Method | Endpoint                   | Description     | Request Example                              |
| ------ | -------------------------- | --------------- | -------------------------------------------- |
| GET    | `/api/roles/get-all`     | Get all roles   | -                                            |
| GET    | `/api/roles/get/{id}`    | Get role by ID  | `/api/roles/get/1`                         |
| POST   | `/api/roles/create`      | Create new role | `{"roleName": "Employee"}`                 |
| PUT    | `/api/roles/update/{id}` | Update role     | `{"id": 1, "roleName": "Senior Employee"}` |
| DELETE | `/api/roles/delete/{id}` | Delete role     | `/api/roles/delete/1`                      |

### RBAC Permission Assignment API

| Method | Endpoint                                                | Description                 | Request Example                     |
| ------ | ------------------------------------------------------- | --------------------------- | ----------------------------------- |
| POST   | `/api/rbac/roles/{roleId}/permissions/{permissionId}` | Assign permission to role   | `/api/rbac/roles/1/permissions/8` |
| DELETE | `/api/rbac/roles/{roleId}/permissions/{permissionId}` | Remove permission from role | `/api/rbac/roles/1/permissions/8` |
| GET    | `/api/rbac/roles/{roleId}/permissions`                | Get role permissions        | `/api/rbac/roles/1/permissions`   |
| GET    | `/api/rbac/permissions/{permissionId}/roles`          | Get roles with permission   | `/api/rbac/permissions/8/roles`   |

### Todo API

| Method | Endpoint                   | Description     | Request Example                                                     |
| ------ | -------------------------- | --------------- | ------------------------------------------------------------------- |
| GET    | `/api/todos/get-all`     | Get all todos   | `/api/todos/get-all?currentUserRoleId=1`                          |
| GET    | `/api/todos/get/{id}`    | Get todo by ID  | `/api/todos/get/1?currentUserRoleId=1`                            |
| POST   | `/api/todos/create`      | Create new todo | `{"title": "Leave Application", "createdByRoleId": 1}`            |
| PUT    | `/api/todos/update/{id}` | Update todo     | `{"todoId": 1, "title": "Updated Title", "currentUserRoleId": 1}` |
| DELETE | `/api/todos/delete/{id}` | Delete todo     | `/api/todos/delete/1?currentUserRoleId=1`                         |

### Review Flow API

| Method | Endpoint                                     | Description              | Request Example                                                                           |
| ------ | -------------------------------------------- | ------------------------ | ----------------------------------------------------------------------------------------- |
| GET    | `/api/todoreviews/get-review-todos`        | Get pending review todos | `/api/todoreviews/get-review-todos?currentUserRoleId=2`                                 |
| POST   | `/api/todoreviews/review`                  | Execute review operation | `{"todoId": 1, "reviewerRoleId": 2, "action": "approve", "comment": "Review approved"}` |
| GET    | `/api/todoreviews/review-history/{todoId}` | Get review history       | `/api/todoreviews/review-history/1?currentUserRoleId=3`                                 |

### Request Examples

Using curl to upload files:

# Create Todo

curl -X POST
  https://localhost:7009/api/todos/create
  -H 'Content-Type: application/json'
  -d '{"title": "Leave Application", "createdByRoleId": 1}'

# Execute Review

curl -X POST
  https://localhost:7009/api/todoreviews/review
  -H 'Content-Type: application/json'
  -d '{"todoId": 1, "reviewerRoleId": 2, "action": "approve", "comment": "Review approved"}'

Using Postman:

- Select POST method
- Set URL: `/api/todos/create`
- Choose Body → raw → JSON
- Input JSON request body

## Core Components Explanation

### Controller Layer

Main API controllers, responsible for:

- Receiving HTTP requests
- Parameter validation and error handling
- Calling service layer for business logic
- Returning API responses

### Service Layer

Business logic processing layer, responsible for:

- Implementing business rules
- Permission verification
- Review flow control
- Exception handling

### Repository Layer

Data access layer, responsible for:

- Database operations
- Entity Framework integration
- Data validation and transformation

### Common Layer

Shared components layer, including:

- Data model definitions
- DTO objects
- Enum types
- Shared utility classes

## Review Flow

### Standard Review Process

1. **Create Application** - Employee creates todo, status set to `pending`
2. **First Level Review** - Senior employee reviews, can approve (`in_progress`) or return (`returned`)
3. **Second Level Review** - Supervisor reviews, can approve (`completed`) or return (`returned`)
4. **Complete Process** - Item status changes to `completed`

### Status Transitions

| Current Status | Allowed Action | Next Status | Executing Role  |
| -------------- | -------------- | ----------- | --------------- |
| pending        | approve        | in_progress | Senior Employee |
| pending        | return         | returned    | Senior Employee |
| in_progress    | approve        | completed   | Supervisor      |
| in_progress    | return         | returned    | Supervisor      |
| returned       | resubmit       | pending     | Employee        |

## Database Design

### Roles Table (roles)

| Field Name | Type         | Description                                                    | Index Suggestion                                                       |
| ---------- | ------------ | -------------------------------------------------------------- | ---------------------------------------------------------------------- |
| id         | INT          | Primary key, unique identifier for each role (auto-increment). | Primary key index (automatic)                                          |
| role_name  | VARCHAR(255) | Role name, e.g., "Employee", "Senior Employee", "Supervisor".  | **Unique index (UNIQUE)**, ensures role names are not duplicated |

### Permissions Table (permissions)

| Field Name      | Type         | Description                                                                       | Index Suggestion                                                             |
| --------------- | ------------ | --------------------------------------------------------------------------------- | ---------------------------------------------------------------------------- |
| id              | INT          | Primary key, unique identifier for each permission (auto-increment).              | Primary key index (automatic)                                                |
| permission_name | VARCHAR(255) | Permission name, e.g., "todo_create", "todo_review_level1", "todo_review_level2". | **Unique index (UNIQUE)**, ensures permission names are not duplicated |

### Role Permissions Association Table (role_permissions)

| Field Name    | Type | Description                                                          | Index Suggestion                                                       |
| ------------- | ---- | -------------------------------------------------------------------- | ---------------------------------------------------------------------- |
| role_id       | INT  | Foreign key, references roles table id, represents role.             | **Foreign key index** (composite primary key with permission_id) |
| permission_id | INT  | Foreign key, references permissions table id, represents permission. | **Foreign key index** (composite primary key with role_id)       |

### Todos Table (todos)

| Field Name      | Type         | Description                                                                                          | Index Suggestion                                                                                             |
| --------------- | ------------ | ---------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------ |
| id              | INT          | Primary key, unique identifier for each todo (auto-increment).                                       | Primary key index (automatic)                                                                                |
| title           | VARCHAR(255) | Todo title.                                                                                          | None (unless frequently searching by title)                                                                  |
| status          | VARCHAR(50)  | Current item status, e.g., "pending_review_level1", "pending_review_level2", "approved", "returned". | **Single column index**, used to speed up status filtering queries (like finding pending review items) |
| created_by_role | INT          | Foreign key, references roles table id, represents the role that created this item (e.g., employee). | **Foreign key index**, used to speed up queries by creator role                                        |
| created_at      | TIMESTAMP    | Item creation time.                                                                                  | None (unless frequently sorting by time)                                                                     |

### Reviews Table (reviews)

| Field Name      | Type         | Description                                                                                                             | Index Suggestion                                                 |
| --------------- | ------------ | ----------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| id              | INT          | Primary key, unique identifier for each review record (auto-increment).                                                 | Primary key index (automatic)                                    |
| todo_id         | INT          | Foreign key, references todos table id, represents the reviewed todo.                                                   | Single column index                                              |
| reviewer_role   | INT          | Foreign key, references roles table id, represents the role executing the review (e.g., senior employee or supervisor). | Foreign key index                                                |
| review_level    | INT          | Review level, e.g., 1 (first level, executed by senior employee), 2 (second level, executed by supervisor).             | None (unless frequently filtering by level)                      |
| action          | VARCHAR(50)  | Review action type, e.g., "approve", "return".                                                                          | None (unless frequently filtering by action type)                |
| reviewed_at     | TIMESTAMP    | Review execution time.                                                                                                  | None (unless frequently sorting by time)                         |
| comment         | VARCHAR(255) | Review comment or note, can be null (corresponds to nullable property in C#).                                           | None (unless frequently performing full-text search on comments) |
| previous_status | VARCHAR(50)  | Status before review, can be null (corresponds to nullable property in C#).                                             | None (unless frequently filtering by status)                     |
| new_status      | VARCHAR(50)  | Status after review, can be null (corresponds to nullable property in C#).                                              | None (unless frequently filtering by status)                     |

## Default Data Setup

### Roles Table (roles)

| id | role_name       | Description                                                                                          |
| -- | --------------- | ---------------------------------------------------------------------------------------------------- |
| 1  | Employee        | Responsible for creating todos and initiating review processes.                                      |
| 2  | Senior Employee | Responsible for first-level review, checking item content and deciding whether to approve or return. |
| 3  | Supervisor      | Responsible for second-level review or final approval, making higher-level decisions.                |
| 4  | Administrator   | Responsible for managing system roles and permissions, not directly involved in business reviews.    |

### Permissions Table (permissions)

| id | permission_name    | Description                                                                               |
| -- | ------------------ | ----------------------------------------------------------------------------------------- |
| 8  | todo_create        | Allows creating todos (corresponds to status initialization).                             |
| 9  | todo_review_level1 | Allows executing first-level review (e.g., updating status to "approved" or "returned").  |
| 10 | todo_review_level2 | Allows executing second-level review (e.g., updating status to "approved" or "returned"). |
| 11 | todo_view_own      | Allows viewing todos created by oneself.                                                  |
| 12 | todo_view_level1   | Allows viewing first-level review related items (status: pending and returned).           |
| 13 | todo_view_level2   | Allows viewing second-level review related items (status: in_progress).                   |
| 14 | admin_manage       | Allows managing role and permission configuration (admin only).                           |

### Role Permissions Association Table (role_permissions)

| role_id | permission_id | Description                                                                                 |
| ------- | ------------- | ------------------------------------------------------------------------------------------- |
| 1       | 8             | Employee has todo_create permission, can create todos.                                      |
| 1       | 11            | Employee has todo_view_own permission, can view own created items.                          |
| 2       | 9             | Senior Employee has todo_review_level1 permission, can execute first-level review.          |
| 2       | 12            | Senior Employee has todo_view_level1 permission, can view first-level review related items. |
| 3       | 10            | Supervisor has todo_review_level2 permission, can execute second-level review.              |
| 3       | 13            | Supervisor has todo_view_level2 permission, can view second-level review related items.     |
| 4       | 14            | Administrator has admin_manage permission, can manage roles and permissions.                |

## Enum Design

### Permissions Enum

| Field Name       | Value | Description                                                                              |
| ---------------- | ----- | ---------------------------------------------------------------------------------------- |
| TodoCreate       | 8     | Allows creating todos (corresponds to status initialization)                             |
| TodoReviewLevel1 | 9     | Allows executing first-level review (e.g., updating status to "approved" or "returned")  |
| TodoReviewLevel2 | 10    | Allows executing second-level review (e.g., updating status to "approved" or "returned") |
| TodoViewOwn      | 11    | Allows viewing todos created by oneself                                                  |
| TodoViewLevel1   | 12    | Allows viewing first-level review related items (status: pending, returned)              |
| TodoViewLevel2   | 13    | Allows viewing second-level review related items (status: in_progress)                   |
| AdminManage      | 14    | Allows managing role and permission configuration (admin only)                           |

### Review Action Enum

| Field Name | Type   | Description |
| ---------- | ------ | ----------- |
| Pending    | string | Pending     |
| InProgress | string | In Progress |
| Approved   | string | Approved    |
| Rejected   | string | Rejected    |
| Returned   | string | Returned    |
| Completed  | string | Completed   |
| Cancelled  | string | Cancelled   |

## Project Architecture

### Layered Structure

Common
Controller
Repositories
Services
UnitTest

### Project Type Selection for Each Layer

- **Common Layer**: Class Library
- **Repositories Layer**: Class Library
- **Services Layer**: Class Library
- **Controller Layer**: ASP.NET Core Web Application or Web API
- **UnitTest Layer**: Unit Test Project

### Project Reference Relationships

In Solution Explorer:

Services Project → Right-click → Add → Reference
Check Common, Repositories

Controller Project → Right-click → Reference
Check Services, Common

UnitTest Project → Right-click → Reference
Check all projects to be tested

## Entity Models

### Role Model (Roles.cs)

public class Roles
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<Roles_Permissions> Roles_Permissions { get; set; } = new List<Roles_Permissions>();
}

### Database Context (Context.cs)

using Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories.MyDbContext;

public partial class Context : DbContext
{
    public Context(DbContextOptions `<Context>` options) : base(options)
    { }

    public DbSet`<Roles>` Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity`<Roles>`(entity =>
        {
            entity.HasKey(e => e.RoleId);
            entity.Property(e => e.RoleId)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.RoleName)
                  .IsRequired()
                  .HasMaxLength(255);
        });
    }
}

### Many-to-Many Relationships

#### Association Table Model

public class Roles_Permissions
{
    public int PermissionId { get; set; }

    public int RoleId { get; set; }

    public virtual Roles Role { get; set; } = null!;

    public virtual Permissions Permission { get; set; } = null!;
}

#### Many-to-Many Configuration

modelBuilder.Entity<Roles_Permissions>(entity =>
{
    entity.HasKey(e => new { e.RoleId, e.PermissionId });

    entity.HasOne(e => e.Role)
          .WithMany(r => r.Roles_Permissions)
          .HasForeignKey(e => e.RoleId)
          .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.Permission)
          .WithMany(p => p.Roles_Permissions)
          .HasForeignKey(e => e.PermissionId)
          .OnDelete(DeleteBehavior.Cascade);
});

## Error Handling

System provides complete error handling mechanism:

- Invalid content type
- Permission verification failure
- Form field count exceeds limit
- Data stream processing exceptions
- Directory creation failure
- Database operation exceptions

## Test Scenarios

### Test Scenario 1: Complete Review Process

| Step | API Endpoint                          | HTTP Method | Request Parameters      | Request Body                                                                                      | Expected Status Code | Expected Response                                             | Test Purpose                                |
| ---- | ------------------------------------- | ----------- | ----------------------- | ------------------------------------------------------------------------------------------------- | -------------------- | ------------------------------------------------------------- | ------------------------------------------- |
| 1    | `/api/todos/create`                 | POST        | None                    | `{"title": "Zhang San Leave Application - 2024/01/15", "createdByRoleId": 1}`                   | 201                  | `{"isSuccess": true, "data": {"status": "pending"}}`        | Employee creates leave application          |
| 2    | `/api/todoreviews/get-review-todos` | GET         | `currentUserRoleId=2` | None                                                                                              | 200                  | `{"isSuccess": true, "data": [{"status": "pending"}]}`      | Senior employee views pending review items  |
| 3    | `/api/todoreviews/review`           | POST        | None                    | `{"todoId": 1, "reviewerRoleId": 2, "action": "approve", "comment": "Leave reason reasonable"}` | 200                  | `{"isSuccess": true, "data": {"newStatus": "in_progress"}}` | Senior employee first-level review approval |
| 4    | `/api/todoreviews/get-review-todos` | GET         | `currentUserRoleId=3` | None                                                                                              | 200                  | `{"isSuccess": true, "data": [{"status": "in_progress"}]}`  | Supervisor views pending review items       |
| 5    | `/api/todoreviews/review`           | POST        | None                    | `{"todoId": 1, "reviewerRoleId": 3, "action": "approve", "comment": "Final approval"}`          | 200                  | `{"isSuccess": true, "data": {"newStatus": "completed"}}`   | Supervisor second-level review approval     |
| 6    | `/api/todoreviews/review-history/1` | GET         | `currentUserRoleId=3` | None                                                                                              | 200                  | `{"isSuccess": true, "data": [2 review records]}`           | View complete review history                |

### Test Scenario 2: Return and Modify Process

| Step | API Endpoint                | HTTP Method | Request Parameters | Request Body                                                                                                    | Expected Status Code | Expected Response                                             | Test Purpose                             |
| ---- | --------------------------- | ----------- | ------------------ | --------------------------------------------------------------------------------------------------------------- | -------------------- | ------------------------------------------------------------- | ---------------------------------------- |
| 1    | `/api/todos/create`       | POST        | None               | `{"title": "Li Si Leave Application", "createdByRoleId": 1}`                                                  | 201                  | `{"isSuccess": true, "data": {"status": "pending"}}`        | Employee creates leave application       |
| 2    | `/api/todoreviews/review` | POST        | None               | `{"todoId": 2, "reviewerRoleId": 2, "action": "return", "comment": "Please supplement supporting documents"}` | 200                  | `{"isSuccess": true, "data": {"newStatus": "returned"}}`    | Senior employee returns for modification |
| 3    | `/api/todoreviews/review` | POST        | None               | `{"todoId": 2, "reviewerRoleId": 1, "action": "resubmit", "comment": "Documents supplemented"}`               | 200                  | `{"isSuccess": true, "data": {"newStatus": "pending"}}`     | Employee resubmits                       |
| 4    | `/api/todoreviews/review` | POST        | None               | `{"todoId": 2, "reviewerRoleId": 2, "action": "approve", "comment": "Approved"}`                              | 200                  | `{"isSuccess": true, "data": {"newStatus": "in_progress"}}` | Senior employee approves                 |

### Test Scenario 3: Permission Verification Test

| Step | API Endpoint                | HTTP Method | Request Parameters | Request Body                                                | Expected Status Code | Expected Response                                    | Test Purpose                                       |
| ---- | --------------------------- | ----------- | ------------------ | ----------------------------------------------------------- | -------------------- | ---------------------------------------------------- | -------------------------------------------------- |
| 1    | `/api/todos/create`       | POST        | None               | `{"title": "Test Application", "createdByRoleId": 1}`     | 201                  | `{"isSuccess": true}`                              | Prepare test data                                  |
| 2    | `/api/todoreviews/review` | POST        | None               | `{"todoId": 3, "reviewerRoleId": 1, "action": "approve"}` | 403                  | `{"isSuccess": false, "message": "Access denied"}` | Employee has no review permission                  |
| 3    | `/api/todoreviews/review` | POST        | None               | `{"todoId": 3, "reviewerRoleId": 2, "action": "approve"}` | 200                  | `{"isSuccess": true}`                              | Senior employee first-level review successful      |
| 4    | `/api/todoreviews/review` | POST        | None               | `{"todoId": 3, "reviewerRoleId": 2, "action": "approve"}` | 400                  | `{"isSuccess": false}`                             | Senior employee cannot perform second-level review |

# 權限 + 審核流程系統

這是一個基於 ASP.NET Core 的權限管理與多級審核流程系統。系統採用角色基礎權限控制(RBAC)設計，支援多層級審核工作流程，確保業務流程的安全性和可追溯性。

## 功能特點

- ✅ 角色基礎權限控制 (RBAC)
- 🔒 多級審核流程管理
- 📦 完整的 CRUD 操作
- 🚀 RESTful API 設計
- ⚡ 權限驗證與存取控制
- 🎯 審核歷史追蹤

## 技術堆疊

- **後端框架**: ASP.NET Core
- **資料庫**: Entity Framework Core
- **語言**: C#
- **架構**: 分層架構 (Controller-Service-Repository)
- **驗證**: 自訂權限驗證機制

## 快速開始

### 系統需求

- .NET 8.0
- 支援的資料庫 (SQL Server/MySQL/PostgreSQL)
- 本地檔案系統寫入權限

### 安裝步驟

1. **複製專案**

git clone https://github.com/lauchiwai/rbac_review.git
cdrbac_review

2. **還原 NuGet 套件**

dotnet restore

3. **設定資料庫連線**

更新 `appsettings.json` 中的連線字串：

{
  "ConnectionStrings": {
    "DefaultConnection": "Your_Connection_String"
  }
}

4. **執行資料庫遷移**

dotnet ef migrations add InitialCreate --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

dotnet ef database update --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

5. **執行應用程式**

dotnet run

## API 文件

### 權限管理 API

| 方法   | 端點                             | 描述             | 請求範例                                       |
| ------ | -------------------------------- | ---------------- | ---------------------------------------------- |
| GET    | `/api/permissions/get-all`     | 取得所有權限     | -                                              |
| GET    | `/api/permissions/get/{id}`    | 根據 ID 取得權限 | `/api/permissions/get/1`                     |
| POST   | `/api/permissions/create`      | 建立新權限       | `{"permissionName": "todo_create"}`          |
| PUT    | `/api/permissions/update/{id}` | 更新權限         | `{"id": 1, "permissionName": "todo_create"}` |
| DELETE | `/api/permissions/delete/{id}` | 刪除權限         | `/api/permissions/delete/1`                  |

### 角色管理 API

| 方法   | 端點                       | 描述             | 請求範例                              |
| ------ | -------------------------- | ---------------- | ------------------------------------- |
| GET    | `/api/roles/get-all`     | 取得所有角色     | -                                     |
| GET    | `/api/roles/get/{id}`    | 根據 ID 取得角色 | `/api/roles/get/1`                  |
| POST   | `/api/roles/create`      | 建立新角色       | `{"roleName": "員工"}`              |
| PUT    | `/api/roles/update/{id}` | 更新角色         | `{"id": 1, "roleName": "資深員工"}` |
| DELETE | `/api/roles/delete/{id}` | 刪除角色         | `/api/roles/delete/1`               |

### RBAC 權限分配 API

| 方法   | 端點                                                    | 描述               | 請求範例                            |
| ------ | ------------------------------------------------------- | ------------------ | ----------------------------------- |
| POST   | `/api/rbac/roles/{roleId}/permissions/{permissionId}` | 分配權限給角色     | `/api/rbac/roles/1/permissions/8` |
| DELETE | `/api/rbac/roles/{roleId}/permissions/{permissionId}` | 從角色移除權限     | `/api/rbac/roles/1/permissions/8` |
| GET    | `/api/rbac/roles/{roleId}/permissions`                | 取得角色權限       | `/api/rbac/roles/1/permissions`   |
| GET    | `/api/rbac/permissions/{permissionId}/roles`          | 取得擁有權限的角色 | `/api/rbac/permissions/8/roles`   |

### 待辦事項 API

| 方法   | 端點                       | 描述                 | 請求範例                                                       |
| ------ | -------------------------- | -------------------- | -------------------------------------------------------------- |
| GET    | `/api/todos/get-all`     | 取得所有待辦事項     | `/api/todos/get-all?currentUserRoleId=1`                     |
| GET    | `/api/todos/get/{id}`    | 根據 ID 取得待辦事項 | `/api/todos/get/1?currentUserRoleId=1`                       |
| POST   | `/api/todos/create`      | 建立新待辦事項       | `{"title": "請假申請", "createdByRoleId": 1}`                |
| PUT    | `/api/todos/update/{id}` | 更新待辦事項         | `{"todoId": 1, "title": "更新標題", "currentUserRoleId": 1}` |
| DELETE | `/api/todos/delete/{id}` | 刪除待辦事項         | `/api/todos/delete/1?currentUserRoleId=1`                    |

### 審核流程 API

| 方法 | 端點                                         | 描述           | 請求範例                                                                           |
| ---- | -------------------------------------------- | -------------- | ---------------------------------------------------------------------------------- |
| GET  | `/api/todoreviews/get-review-todos`        | 取得待審核事項 | `/api/todoreviews/get-review-todos?currentUserRoleId=2`                          |
| POST | `/api/todoreviews/review`                  | 執行審核操作   | `{"todoId": 1, "reviewerRoleId": 2, "action": "approve", "comment": "審核通過"}` |
| GET  | `/api/todoreviews/review-history/{todoId}` | 取得審核歷史   | `/api/todoreviews/review-history/1?currentUserRoleId=3`                          |

### 請求範例

使用 curl 上傳檔案：

# 建立待辦事項

curl -X POST
  https://localhost:7009/api/todos/create
  -H 'Content-Type: application/json'
  -d '{"title": "請假申請", "createdByRoleId": 1}'

# 執行審核

curl -X POST
  https://localhost:7009/api/todoreviews/review
  -H 'Content-Type: application/json'
  -d '{"todoId": 1, "reviewerRoleId": 2, "action": "approve", "comment": "審核通過"}'

使用 Postman：

- 選擇 POST 方法
- 設定 URL: `/api/todos/create`
- 選擇 Body → raw → JSON
- 輸入 JSON 請求體

## 核心組件說明

### 控制器層 (Controller)

主要 API 控制器，負責：

- 接收 HTTP 請求
- 參數驗證與錯誤處理
- 呼叫服務層處理業務邏輯
- 回傳 API 回應

### 服務層 (Services)

業務邏輯處理層，負責：

- 實現業務規則
- 權限驗證
- 審核流程控制
- 例外處理

### 儲存庫層 (Repositories)

資料存取層，負責：

- 資料庫操作
- Entity Framework 整合
- 資料驗證與轉換

### 通用層 (Common)

共用元件層，包含：

- 資料模型定義
- DTO 物件
- 枚舉類型
- 共用工具類

## 審核流程

### 標準審核流程

1. **建立申請** - 員工建立待辦事項，狀態設為 `pending`
2. **一級審核** - 資深員工審核，可批准(`in_progress`)或退回(`returned`)
3. **二級審核** - 主管審核，可批准(`completed`)或退回(`returned`)
4. **完成流程** - 事項狀態變更為 `completed`

### 狀態轉換

| 當前狀態    | 允許操作 | 下一狀態    | 執行角色 |
| ----------- | -------- | ----------- | -------- |
| pending     | approve  | in_progress | 資深員工 |
| pending     | return   | returned    | 資深員工 |
| in_progress | approve  | completed   | 主管     |
| in_progress | return   | returned    | 主管     |
| returned    | resubmit | pending     | 員工     |

## 資料庫設計

### 角色表 (roles)

| 欄位名稱  | 類型         | 功能描述                                    | 索引建議                                         |
| --------- | ------------ | ------------------------------------------- | ------------------------------------------------ |
| id        | INT          | 主鍵，唯一識別每個角色（自動增量）。        | 主鍵索引（自動）                                 |
| role_name | VARCHAR(255) | 角色名稱，例如 "員工"、"資深員工"、"主管"。 | **唯一索引（UNIQUE）**，確保角色名稱不重複 |

### 權限表 (permissions)

| 欄位名稱        | 類型         | 功能描述                                                                   | 索引建議                                         |
| --------------- | ------------ | -------------------------------------------------------------------------- | ------------------------------------------------ |
| id              | INT          | 主鍵，唯一識別每個權限（自動增量）。                                       | 主鍵索引（自動）                                 |
| permission_name | VARCHAR(255) | 權限名稱，例如 "todo_create"、"todo_review_level1"、"todo_review_level2"。 | **唯一索引（UNIQUE）**，確保權限名稱不重複 |

### 角色權限關聯表 (role_permissions)

| 欄位名稱      | 類型 | 功能描述                                     | 索引建議                                            |
| ------------- | ---- | -------------------------------------------- | --------------------------------------------------- |
| role_id       | INT  | 外鍵，關聯到 roles 表的 id，表示角色。       | **外鍵索引**（與 permission_id 組成複合主鍵） |
| permission_id | INT  | 外鍵，關聯到 permissions 表的 id，表示權限。 | **外鍵索引**（與 role_id 組成複合主鍵）       |

### 待辦事項表 (todos)

| 欄位名稱        | 類型         | 功能描述                                                                                        | 索引建議                                                       |
| --------------- | ------------ | ----------------------------------------------------------------------------------------------- | -------------------------------------------------------------- |
| id              | INT          | 主鍵，唯一識別每個待辦事項（自動增量）。                                                        | 主鍵索引（自動）                                               |
| title           | VARCHAR(255) | 待辦事項的標題。                                                                                | 無（除非經常按標題搜索）                                       |
| status          | VARCHAR(50)  | 事項的當前狀態，例如 "pending_review_level1"、"pending_review_level2"、"approved"、"returned"。 | **單欄位索引**，用於加速狀態過濾查詢（如查找待審核事項） |
| created_by_role | INT          | 外鍵，關聯到 roles 表的 id，表示創建此事項的角色（例如員工）。                                  | **外鍵索引**，用於加速按創建角色查詢                     |
| created_at      | TIMESTAMP    | 事項的創建時間。                                                                                | 無（除非經常按時間排序）                                       |

### 審核記錄表 (reviews)

| 欄位名稱        | 類型         | 功能描述                                                               | 索引建議                             |
| --------------- | ------------ | ---------------------------------------------------------------------- | ------------------------------------ |
| id              | INT          | 主鍵，唯一識別每筆審核記錄（自動增量）。                               | 主鍵索引（自動）                     |
| todo_id         | INT          | 外鍵，關聯到 todos 表的 id，表示被審核的待辦事項。                     | 單欄位索引                           |
| reviewer_role   | INT          | 外鍵，關聯到 roles 表的 id，表示執行審核的角色（例如資深員工或主管）。 | 外鍵索引                             |
| review_level    | INT          | 審核級別，例如 1（第一級，由資深員工執行）、2（第二級，由主管執行）。  | 無（除非經常按級別過濾）             |
| action          | VARCHAR(50)  | 審核操作類型，例如 "approve"、"return"。                               | 無（除非經常按操作類型過濾）         |
| reviewed_at     | TIMESTAMP    | 審核執行時間。                                                         | 無（除非經常按時間排序）             |
| comment         | VARCHAR(255) | 審核評論或備註，可為空（對應 C# 中的 nullable 屬性）。                 | 無（除非經常按評論內容進行全文搜索） |
| previous_status | VARCHAR(50)  | 審核前的狀態，可為空（對應 C# 中的 nullable 屬性）。                   | 無（除非經常按狀態過濾）             |
| new_status      | VARCHAR(50)  | 審核後的狀態，可為空（對應 C# 中的 nullable 屬性）。                   | 無（除非經常按狀態過濾）             |

## 預設資料設定

### 角色表 (roles)

| id | role_name | 功能描述                                         |
| -- | --------- | ------------------------------------------------ |
| 1  | 員工      | 負責創建待辦事項，並發起審核流程。               |
| 2  | 資深員工  | 負責一級審核，檢查事項內容並決定是否批准或退回。 |
| 3  | 主管      | 負責二級審核或最終批准，進行更高層次的決策。     |
| 4  | 管理員    | 負責管理系統角色和權限，不直接參與業務審核。     |

### 權限表 (permissions)

| id | permission_name    | 功能描述                                                      |
| -- | ------------------ | ------------------------------------------------------------- |
| 8  | todo_create        | 允許創建待辦事項（對應狀態初始化）。                          |
| 9  | todo_review_level1 | 允許執行一級審核（例如更新狀態為 "approved" 或 "returned"）。 |
| 10 | todo_review_level2 | 允許執行二級審核（例如更新狀態為 "approved" 或 "returned"）。 |
| 11 | todo_view_own      | 允許查看自己創建的待辦事項。                                  |
| 12 | todo_view_level1   | 允許查看一級審核相關事項（狀態為 pending 和 returned）。      |
| 13 | todo_view_level2   | 允許查看二級審核相關事項（狀態為 in_progress）。              |
| 14 | admin_manage       | 允許管理角色和權限配置（僅管理員使用）。                      |

### 角色權限關聯表 (role_permissions)

| role_id | permission_id | 說明                                                         |
| ------- | ------------- | ------------------------------------------------------------ |
| 1       | 8             | 員工擁有 todo_create 權限，可創建待辦事項。                  |
| 1       | 11            | 員工擁有 todo_view_own 權限，可查看自己創建的事項。          |
| 2       | 9             | 資深員工擁有 todo_review_level1 權限，可執行一級審核。       |
| 2       | 12            | 資深員工擁有 todo_view_level1 權限，可查看一級審核相關事項。 |
| 3       | 10            | 主管擁有 todo_review_level2 權限，可執行二級審核。           |
| 3       | 13            | 主管擁有 todo_view_level2 權限，可查看二級審核相關事項。     |
| 4       | 14            | 管理員擁有 admin_manage 權限，可管理角色和權限。             |

## 枚舉設計

### 權限枚舉 (Permissions)

| 欄位名稱         | 值 | 功能描述                                                    |
| ---------------- | -- | ----------------------------------------------------------- |
| TodoCreate       | 8  | 允許創建待辦事項（對應狀態初始化）                          |
| TodoReviewLevel1 | 9  | 允許執行一級審核（例如更新狀態為 "approved" 或 "returned"） |
| TodoReviewLevel2 | 10 | 允許執行二級審核（例如更新狀態為 "approved" 或 "returned"） |
| TodoViewOwn      | 11 | 允許查看自己創建的待辦事項                                  |
| TodoViewLevel1   | 12 | 允許查看一級審核相關事項（狀態：pending, returned）         |
| TodoViewLevel2   | 13 | 允許查看二級審核相關事項（狀態：in_progress）               |
| AdminManage      | 14 | 允許管理角色和權限配置（僅管理員使用）                      |

### 審核操作枚舉 (ReviewAction)

| 欄位名稱   | 類型   | 功能描述 |
| ---------- | ------ | -------- |
| Pending    | string | 待處理   |
| InProgress | string | 進行中   |
| Approved   | string | 已批准   |
| Rejected   | string | 已拒絕   |
| Returned   | string | 已退回   |
| Completed  | string | 已完成   |
| Cancelled  | string | 已取消   |

## 專案架構

### 分層結構

Common
Controller
Repositories
Services
UnitTest

### 各層級專案類型選擇

- **Common 層**：類別庫
- **Repositories 層**：類別庫
- **Services 層**：類別庫
- **Controller 層**：ASP.NET Core Web 應用程式或 Web API
- **UnitTest 層**：單元測試專案

### 專案參考關係

在方案總管中操作：

Services 專案 → 右鍵 → 新增 → 參考
勾選 Common、Repositories

Controller 專案 → 右鍵 → 參考
勾選 Services、Common

UnitTest 專案 → 右鍵 → 參考
勾選所有要測試的專案

## 實體模型

### 角色模型 (Roles.cs)

public class Roles
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<Roles_Permissions> Roles_Permissions { get; set; } = new List<Roles_Permissions>();
}

### 資料庫上下文 (Context.cs)

using Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories.MyDbContext;

public partial class Context : DbContext
{
    public Context(DbContextOptions `<Context>` options) : base(options)
    { }

    public DbSet`<Roles>` Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity`<Roles>`(entity =>
        {
            entity.HasKey(e => e.RoleId);
            entity.Property(e => e.RoleId)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.RoleName)
                  .IsRequired()
                  .HasMaxLength(255);
        });
    }
}

### 多對多關聯

#### 關聯表模型

public class Roles_Permissions
{
    public int PermissionId { get; set; }

    public int RoleId { get; set; }

    public virtual Roles Role { get; set; } = null!;

    public virtual Permissions Permission { get; set; } = null!;
}

#### 多對多設定

modelBuilder.Entity<Roles_Permissions>(entity =>
{
    entity.HasKey(e => new { e.RoleId, e.PermissionId });

    entity.HasOne(e => e.Role)
          .WithMany(r => r.Roles_Permissions)
          .HasForeignKey(e => e.RoleId)
          .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.Permission)
          .WithMany(p => p.Roles_Permissions)
          .HasForeignKey(e => e.PermissionId)
          .OnDelete(DeleteBehavior.Cascade);
});

## 錯誤處理

系統提供完整的錯誤處理機制：

- 無效的內容類型
- 權限驗證失敗
- 表單欄位數量超過限制
- 資料流處理例外
- 目錄建立失敗
- 資料庫操作例外

## 測試場景

### 測試場景 1: 完整審核流程

| 步驟 | API 端點                              | HTTP 方法 | 請求參數                | 請求體                                                                                 | 預期狀態碼 | 預期響應                                                      | 測試目的               |
| ---- | ------------------------------------- | --------- | ----------------------- | -------------------------------------------------------------------------------------- | ---------- | ------------------------------------------------------------- | ---------------------- |
| 1    | `/api/todos/create`                 | POST      | 無                      | `{"title": "張三請假申請 - 2024/01/15", "createdByRoleId": 1}`                       | 201        | `{"isSuccess": true, "data": {"status": "pending"}}`        | 員工創建請假申請       |
| 2    | `/api/todoreviews/get-review-todos` | GET       | `currentUserRoleId=2` | 無                                                                                     | 200        | `{"isSuccess": true, "data": [{"status": "pending"}]}`      | 資深員工查看待審核事項 |
| 3    | `/api/todoreviews/review`           | POST      | 無                      | `{"todoId": 1, "reviewerRoleId": 2, "action": "approve", "comment": "請假事由合理"}` | 200        | `{"isSuccess": true, "data": {"newStatus": "in_progress"}}` | 資深員工一級審核批准   |
| 4    | `/api/todoreviews/get-review-todos` | GET       | `currentUserRoleId=3` | 無                                                                                     | 200        | `{"isSuccess": true, "data": [{"status": "in_progress"}]}`  | 主管查看待審核事項     |
| 5    | `/api/todoreviews/review`           | POST      | 無                      | `{"todoId": 1, "reviewerRoleId": 3, "action": "approve", "comment": "最終批准"}`     | 200        | `{"isSuccess": true, "data": {"newStatus": "completed"}}`   | 主管二級審核批准       |
| 6    | `/api/todoreviews/review-history/1` | GET       | `currentUserRoleId=3` | 無                                                                                     | 200        | `{"isSuccess": true, "data": [2條審核記錄]}`                | 查看完整審核歷史       |

### 測試場景 2: 退回修改流程

| 步驟 | API 端點                    | HTTP 方法 | 請求參數 | 請求體                                                                                  | 預期狀態碼 | 預期響應                                                      | 測試目的         |
| ---- | --------------------------- | --------- | -------- | --------------------------------------------------------------------------------------- | ---------- | ------------------------------------------------------------- | ---------------- |
| 1    | `/api/todos/create`       | POST      | 無       | `{"title": "李四請假申請", "createdByRoleId": 1}`                                     | 201        | `{"isSuccess": true, "data": {"status": "pending"}}`        | 員工創建請假申請 |
| 2    | `/api/todoreviews/review` | POST      | 無       | `{"todoId": 2, "reviewerRoleId": 2, "action": "return", "comment": "請補充證明文件"}` | 200        | `{"isSuccess": true, "data": {"newStatus": "returned"}}`    | 資深員工退回修改 |
| 3    | `/api/todoreviews/review` | POST      | 無       | `{"todoId": 2, "reviewerRoleId": 1, "action": "resubmit", "comment": "已補充文件"}`   | 200        | `{"isSuccess": true, "data": {"newStatus": "pending"}}`     | 員工重新提交     |
| 4    | `/api/todoreviews/review` | POST      | 無       | `{"todoId": 2, "reviewerRoleId": 2, "action": "approve", "comment": "批准"}`          | 200        | `{"isSuccess": true, "data": {"newStatus": "in_progress"}}` | 資深員工批准     |

### 測試場景 3: 權限驗證測試

| 步驟 | API 端點                    | HTTP 方法 | 請求參數 | 請求體                                                      | 預期狀態碼 | 預期響應                                             | 測試目的             |
| ---- | --------------------------- | --------- | -------- | ----------------------------------------------------------- | ---------- | ---------------------------------------------------- | -------------------- |
| 1    | `/api/todos/create`       | POST      | 無       | `{"title": "測試申請", "createdByRoleId": 1}`             | 201        | `{"isSuccess": true}`                              | 準備測試資料         |
| 2    | `/api/todoreviews/review` | POST      | 無       | `{"todoId": 3, "reviewerRoleId": 1, "action": "approve"}` | 403        | `{"isSuccess": false, "message": "Access denied"}` | 員工無審核權限       |
| 3    | `/api/todoreviews/review` | POST      | 無       | `{"todoId": 3, "reviewerRoleId": 2, "action": "approve"}` | 200        | `{"isSuccess": true}`                              | 資深員工一級審核成功 |
| 4    | `/api/todoreviews/review` | POST      | 無       | `{"todoId": 3, "reviewerRoleId": 2, "action": "approve"}` | 400        | `{"isSuccess": false}`                             | 資深員工無法二級審核 |
