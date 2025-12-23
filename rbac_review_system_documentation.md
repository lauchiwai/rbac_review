
# RBAC + Review GitHub Readme

A complete review process management system based on Role-Based Access Control (RBAC), combining permission management and multi-level review workflows.

## Project Overview

This is an enterprise-level review process management system that provides:

- Complete RBAC permission management: Three-level permission control of users, roles, and permissions
- Flexible review process configuration: Customizable multi-stage review templates
- State machine-driven review logic: State transition rules based on the StageTransitions table
- Complete review history tracking: All review actions have complete records

### Core Features

1. Permission management: CRUD operation permissions and roles
2. User management: User creation and role assignment
3. Review template management: Custom review process stages
4. Todo item review: Multi-stage review process execution
5. Review history query: Complete review process tracking

## Project Layer Structure

Common
Controller
Repositories
Services
UnitTest

### Project Type Selection for Each Layer

Common layer: Choose "Class Library"
Repositories layer: Choose "Class Library"
Services layer: Choose "Class Library"
Controller layer: Choose "ASP.NET Core Web Application" or "Web API"
UnitTest layer: Choose "Unit Test Project"

### Technology Stack

- Backend framework: ASP.NET Core 8.0+
- Database: SQL Server / Entity Framework Core
- API design: RESTful API
- Testing framework: xUnit
- Dependency injection: Built-in DI container

## Database Design

### Core Entity Relationship Diagram

```
┌─────────┐     ┌──────────────┐     ┌──────────────┐
│  Users  │─────│ Users_Roles  │─────│    Roles     │
└─────────┘     └──────────────┘     └──────────────┘
     │                                    │
     │                                    │
┌─────────┐                       ┌──────────────┐
│TodoLists│                       │ Permissions  │
└─────────┘                       └──────────────┘
     │                                    │
     │                                    │
┌─────────┐     ┌──────────────┐     ┌───────────────┐
│ Reviews │─────│ReviewStages  │─────│ReviewTemplates│
└─────────┘     └──────────────┘     └───────────────┘
                      │
                      │
                ┌────────────────┐
                │StageTransitions│
                └────────────────┘
```


## Quick Start

### Environment Requirements

- .NET 8.0 SDK or higher
- SQL Server 2016+ or SQL Server Express
- Visual Studio 2022 or VS Code

### Installation Steps

1. Clone the project
   git clone https://github.com/lauchiwai/rbac_review.git
   cd rbac-reviews
2. Restore NuGet packages
   dotnet restore
3. Configure database connection
   Edit appsettings.json:
   {
   "ConnectionStrings": {
   "DefaultConnection": "Server=localhost;Database=RbacReviewsDB;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   }
4. Run database migrations
   Create migration:
   dotnet ef migrations add InitialCreate --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

   Update database:
   dotnet ef database update --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context
5. Run the application
   dotnet run --project .\rbac_reviews\rbac_reviews.csproj
6. Test API
   Swagger UI: https://localhost:5001/swagger
   Postman collection: Import examples

## Database Migration Commands

### Create new migration

dotnet ef migrations add [DescriptiveName] --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

### Update database

dotnet ef database update --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

### Common migration operations

Create initial migration:
dotnet ef migrations add InitialCreate --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

Update to specific migration:
dotnet ef database update 20240101000000_InitialCreate --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

Remove recent migration:
dotnet ef migrations remove --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

## Integration Testing

## Review Process Example

### Multiple return process

Create → Level 1 approval → Level 2 return → Level 1 return → Creator supplement → Resubmit → Level 1 approval → Level 2 approval → Complete

# Pre-review Setup

## API 1: Create Level 2 Review Template

| Field        | Value                                                                                                                                                                             | Description                                    |
| ------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------- |
| HTTP Method  | POST                                                                                                                                                                              |                                                |
| URL          | /api/ReviewTemplate/CreateLevel2Review                                                                                                                                            |                                                |
| Content-Type | application/json                                                                                                                                                                  |                                                |
| Request Body | `{ "userId": 106, "templateName": "Project Review Process", "description": "Standard two-level review process, includes level 1 and level 2 review", "level1ReviewerId": 102 }` | Admin(User106) creates level 2 review template |

## API 2: Create Todo Item

| Field        | Value                                                                | Description                         |
| ------------ | -------------------------------------------------------------------- | ----------------------------------- |
| HTTP Method  | POST                                                                 |                                     |
| URL          | /api/Todo/CreateTodo                                                 |                                     |
| Content-Type | application/json                                                     |                                     |
| Request Body | `{ "userId": 101, "title": "Q1 Project Report", "templateId": 1 }` | Employee(User101) creates todo item |

# Review Process

## Step 1: User102 views initial pending review items

| Field        | Value                             | Description                        |
| ------------ | --------------------------------- | ---------------------------------- |
| HTTP Method  | GET                               |                                    |
| URL          | /api/Review/GetPendingReviews/102 |                                    |
| Content-Type | None                              |                                    |
| Request Body | None                              | User102 views pending review items |

## Step 2: User102 approves todo item 4

| Field        | Value                                                                                                                           | Description                                                          |
| ------------ | ------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------- |
| HTTP Method  | POST                                                                                                                            |                                                                      |
| URL          | /api/Review/ExecuteApproveAction                                                                                                |                                                                      |
| Content-Type | application/json                                                                                                                |                                                                      |
| Request Body | `{ "userId": 102, "todoId": 1, "comment": "Initial review passed, please proceed to level 2 review", "nextReviewerId": 104 }` | User102 approves todo item, specifies next stage reviewer as User104 |

## Step 3: User104 views pending review items

| Field        | Value                             | Description                        |
| ------------ | --------------------------------- | ---------------------------------- |
| HTTP Method  | GET                               |                                    |
| URL          | /api/Review/GetPendingReviews/104 |                                    |
| Content-Type | None                              |                                    |
| Request Body | None                              | User104 views pending review items |

## Step 4: User104 returns todo item 4

| Field        | Value                                                                                          | Description                                 |
| ------------ | ---------------------------------------------------------------------------------------------- | ------------------------------------------- |
| HTTP Method  | POST                                                                                           |                                             |
| URL          | /api/Review/ExecuteReturnAction                                                                |                                             |
| Content-Type | application/json                                                                               |                                             |
| Request Body | `{ "userId": 104, "todoId": 1, "comment": "Project schedule needs more detailed planning" }` | User104 returns todo item to level 1 review |

## Step 5: User102 views pending review items (returned to level 1 review status)

| Field        | Value                             | Description                      |
| ------------ | --------------------------------- | -------------------------------- |
| HTTP Method  | GET                               |                                  |
| URL          | /api/Review/GetPendingReviews/102 |                                  |
| Content-Type | None                              |                                  |
| Request Body | None                              | User102 views returned todo item |

## Step 6: User102 returns todo item from level 1 review stage

| Field        | Value                             | Description                      |
| ------------ | --------------------------------- | -------------------------------- |
| HTTP Method  | GET                               |                                  |
| URL          | /api/Review/GetPendingReviews/101 |                                  |
| Content-Type | None                              |                                  |
| Request Body | None                              | User101 views returned todo item |

## Step 7: User101 views pending review items (returned to creator status)

| Field        | Value                             | Description                      |
| ------------ | --------------------------------- | -------------------------------- |
| HTTP Method  | GET                               |                                  |
| URL          | /api/Review/GetPendingReviews/101 |                                  |
| Content-Type | None                              |                                  |
| Request Body | None                              | User101 views returned todo item |

## Step 8: User101 resubmits todo item

| Field        | Value                                                                                                  | Description                 |
| ------------ | ------------------------------------------------------------------------------------------------------ | --------------------------- |
| HTTP Method  | POST                                                                                                   |                             |
| URL          | /api/Review/ExecuteResubmitAction                                                                      |                             |
| Content-Type | application/json                                                                                       |                             |
| Request Body | `{ "userId": 101, "todoId": 1, "comment": "Added detailed budget breakdown and schedule planning" }` | User101 resubmits todo item |

## Step 9: User102 views pending review items (after resubmission, level 1 review)

| Field        | Value                             | Description                         |
| ------------ | --------------------------------- | ----------------------------------- |
| HTTP Method  | GET                               |                                     |
| URL          | /api/Review/GetPendingReviews/102 |                                     |
| Content-Type | None                              |                                     |
| Request Body | None                              | User102 views resubmitted todo item |

## Step 10: User102 approves todo item

| Field        | Value                                                                                                                | Description                |
| ------------ | -------------------------------------------------------------------------------------------------------------------- | -------------------------- |
| HTTP Method  | POST                                                                                                                 |                            |
| URL          | /api/Review/ExecuteApproveAction                                                                                     |                            |
| Content-Type | application/json                                                                                                     |                            |
| Request Body | `{ "userId": 102, "todoId": 1, "comment": "Supplemental materials complete, agree to submit for level 2 review" }` | User102 approves todo item |

## Step 11: User104 views pending review items (enters level 2 review again)

| Field        | Value                             | Description                        |
| ------------ | --------------------------------- | ---------------------------------- |
| HTTP Method  | GET                               |                                    |
| URL          | /api/Review/GetPendingReviews/104 |                                    |
| Content-Type | None                              |                                    |
| Request Body | None                              | User104 views pending review items |

## Step 12: User104 approves todo item (completes)

| Field        | Value                                                                                              | Description                                          |
| ------------ | -------------------------------------------------------------------------------------------------- | ---------------------------------------------------- |
| HTTP Method  | POST                                                                                               |                                                      |
| URL          | /api/Review/ExecuteApproveAction                                                                   |                                                      |
| Content-Type | application/json                                                                                   |                                                      |
| Request Body | `{ "userId": 104, "todoId": 1, "comment": "Project planning complete, approved for execution" }` | User104 approves todo item, completes review process |

### Table Structures

# Table

## 1. Roles (Role Table)

| Field Name        | Type                | Description                                                         | Index Recommendation            | C# Model Mapping                                                                  |
| ----------------- | ------------------- | ------------------------------------------------------------------- | ------------------------------- | --------------------------------------------------------------------------------- |
| RoleId            | INT                 | Primary key, uniquely identifies each role (auto-increment).        | Primary key index (automatic)   | `public int RoleId { get; set; }`                                               |
| RoleName          | VARCHAR(255)        | Role name, e.g., "Employee", "Senior Employee", "Manager".          | **Unique index (UNIQUE)** | `public string RoleName { get; set; } = null!;`                                 |
| Roles_Permissions | Navigation property | One-to-many relationship (one role can have multiple permissions).  | None                            | `public virtual ICollection<Roles_Permissions> Roles_Permissions { get; set; }` |
| Users_Roles       | Navigation property | One-to-many relationship (one role can be owned by multiple users). | None                            | `public virtual ICollection<Users_Roles> Users_Roles { get; set; }`             |

## 2. Permissions (Permission Table)

| Field Name        | Type                | Description                                                               | Index Recommendation            | C# Model Mapping                                                                  |
| ----------------- | ------------------- | ------------------------------------------------------------------------- | ------------------------------- | --------------------------------------------------------------------------------- |
| PermissionId      | INT                 | Primary key, uniquely identifies each permission (auto-increment).        | Primary key index (automatic)   | `public int PermissionId { get; set; }`                                         |
| PermissionName    | VARCHAR(255)        | Permission name, e.g., "todo_create", "todo_review_level1".               | **Unique index (UNIQUE)** | `public string PermissionName { get; set; } = null!;`                           |
| Roles_Permissions | Navigation property | One-to-many relationship (one permission can be owned by multiple roles). | None                            | `public virtual ICollection<Roles_Permissions> Roles_Permissions { get; set; }` |

## 3. Roles_Permissions (Role-Permission Association Table)

| Field Name   | Type                | Description                                                                       | Index Recommendation                                | C# Model Mapping                                                 |
| ------------ | ------------------- | --------------------------------------------------------------------------------- | --------------------------------------------------- | ---------------------------------------------------------------- |
| RoleId       | INT                 | Foreign key, references RoleId in roles table, represents role.                   | **Foreign key index** (composite primary key) | `public int RoleId { get; set; }`                              |
| PermissionId | INT                 | Foreign key, references PermissionId in permissions table, represents permission. | **Foreign key index** (composite primary key) | `public int PermissionId { get; set; }`                        |
| Role         | Navigation property | References Roles model.                                                           | None                                                | `public virtual Roles Role { get; set; } = null!;`             |
| Permission   | Navigation property | References Permissions model.                                                     | None                                                | `public virtual Permissions Permission { get; set; } = null!;` |

**Composite primary key:** (RoleId, PermissionId)

## 4. Users (User Table)

| Field Name  | Type                | Description                                                  | Index Recommendation          | C# Model Mapping                                                      |
| ----------- | ------------------- | ------------------------------------------------------------ | ----------------------------- | --------------------------------------------------------------------- |
| UserId      | INT                 | Primary key, uniquely identifies each user (auto-increment). | Primary key index (automatic) | `public int UserId { get; set; }`                                   |
| CreatedAt   | DATETIME            | User creation time, defaults to current time.                | None                          | `public DateTime CreatedAt { get; set; }`                           |
| Users_Roles | Navigation property | One-to-many relationship (one user can have multiple roles). | None                          | `public virtual ICollection<Users_Roles> Users_Roles { get; set; }` |

## 5. Users_Roles (User-Role Association Table)

| Field Name | Type                | Description                                                     | Index Recommendation                                | C# Model Mapping                                     |
| ---------- | ------------------- | --------------------------------------------------------------- | --------------------------------------------------- | ---------------------------------------------------- |
| UserId     | INT                 | Foreign key, references UserId in users table, represents user. | **Foreign key index** (composite primary key) | `public int UserId { get; set; }`                  |
| RoleId     | INT                 | Foreign key, references RoleId in roles table, represents role. | **Foreign key index** (composite primary key) | `public int RoleId { get; set; }`                  |
| User       | Navigation property | References Users model.                                         | None                                                | `public virtual Users User { get; set; } = null!;` |
| Role       | Navigation property | References Roles model.                                         | None                                                | `public virtual Roles Role { get; set; } = null!;` |

**Composite primary key:** (UserId, RoleId)

## 6. ReviewTemplates (Review Template Table)

| Field Name      | Type                | Description                                                                 | Index Recommendation            | C# Model Mapping                                                       |
| --------------- | ------------------- | --------------------------------------------------------------------------- | ------------------------------- | ---------------------------------------------------------------------- |
| TemplateId      | INT                 | Primary key, uniquely identifies each template (auto-increment).            | Primary key index (automatic)   | `public int TemplateId { get; set; }`                                |
| TemplateName    | VARCHAR(255)        | Template name, e.g., "Standard Two-Level Review".                           | **Unique index (UNIQUE)** | `public string TemplateName { get; set; } = null!;`                  |
| Description     | VARCHAR(500)        | Template description.                                                       | None                            | `public string? Description { get; set; }`                           |
| IsActive        | BOOLEAN             | Whether active (default true).                                              | Index                           | `public bool IsActive { get; set; } = true`                          |
| CreatedAt       | DATETIME            | Creation time (default current time).                                       | None                            | `public DateTime CreatedAt { get; set; }`                            |
| CreatedByUserId | INT                 | Creator user ID (nullable).                                                 | Foreign key index               | `public int? CreatedByUserId { get; set; }`                          |
| CreatedByUser   | Navigation property | References creator user.                                                    | None                            | `public virtual Users? CreatedByUser { get; set; }`                  |
| ReviewStages    | Navigation property | One-to-many relationship (one template can have multiple review stages).    | None                            | `public virtual ICollection<ReviewStage> ReviewStages { get; set; }` |
| TodoLists       | Navigation property | One-to-many relationship (one template can be used by multiple todo items). | None                            | `public virtual ICollection<TodoLists> TodoLists { get; set; }`      |

## 7. ReviewStages (Review Stage Table)

| Field Name             | Type                | Description                                                                        | Index Recommendation                     | C# Model Mapping                                                                   |
| ---------------------- | ------------------- | ---------------------------------------------------------------------------------- | ---------------------------------------- | ---------------------------------------------------------------------------------- |
| StageId                | INT                 | Primary key, uniquely identifies each stage (auto-increment).                      | Primary key index (automatic)            | `public int StageId { get; set; }`                                               |
| TemplateId             | INT                 | Foreign key, references review template.                                           | Foreign key index                        | `public int TemplateId { get; set; }`                                            |
| StageName              | VARCHAR(255)        | Stage name, e.g., "Level 1 Review".                                                | None                                     | `public string StageName { get; set; } = null!;`                                 |
| StageOrder             | INT                 | Stage order (starts from 1).                                                       | Composite index (TemplateId, StageOrder) | `public int StageOrder { get; set; }`                                            |
| RequiredRoleId         | INT                 | Required role ID.                                                                  | Foreign key index                        | `public int RequiredRoleId { get; set; }`                                        |
| SpecificReviewerUserId | INT                 | Specific reviewer user ID.                                                         | Foreign key index                        | `public int SpecificReviewerUserId { get; set; }`                                |
| ReviewTemplate         | Navigation property | References review template.                                                        | None                                     | `public virtual ReviewTemplate ReviewTemplate { get; set; } = null!;`            |
| RequiredRole           | Navigation property | References required role.                                                          | None                                     | `public virtual Roles RequiredRole { get; set; } = null!;`                       |
| SpecificReviewerUser   | Navigation property | References specific reviewer user.                                                 | None                                     | `public virtual Users SpecificReviewerUser { get; set; } = null!;`               |
| TodoLists              | Navigation property | One-to-many relationship (one stage can be current stage for multiple todo items). | None                                     | `public virtual ICollection<TodoLists> TodoLists { get; set; }`                  |
| FromStageTransitions   | Navigation property | One-to-many relationship (transition rules as source stage).                       | None                                     | `public virtual ICollection<StageTransition> FromStageTransitions { get; set; }` |
| ToStageTransitions     | Navigation property | One-to-many relationship (transition rules as target stage).                       | None                                     | `public virtual ICollection<StageTransition> ToStageTransitions { get; set; }`   |
| Reviews                | Navigation property | One-to-many relationship (one stage can have multiple review records).             | None                                     | `public virtual ICollection<Reviews> Reviews { get; set; }`                      |

## 8. StageTransitions (Stage Transition Rules Table)

| Field Name   | Type                | Description                                                                      | Index Recommendation                  | C# Model Mapping                                                |
| ------------ | ------------------- | -------------------------------------------------------------------------------- | ------------------------------------- | --------------------------------------------------------------- |
| TransitionId | INT                 | Primary key, uniquely identifies each transition rule (auto-increment).          | Primary key index (automatic)         | `public int TransitionId { get; set; }`                       |
| StageId      | INT                 | Foreign key, references source stage.                                            | Foreign key index                     | `public int StageId { get; set; }`                            |
| ActionName   | VARCHAR(50)         | Action name, e.g., "approve".                                                    | Composite index (StageId, ActionName) | `public string ActionName { get; set; } = null!;`             |
| NextStageId  | INT                 | Target stage ID (nullable).                                                      | Foreign key index                     | `public int? NextStageId { get; set; }`                       |
| ResultStatus | VARCHAR(50)         | Result status.                                                                   | None                                  | `public string ResultStatus { get; set; } = null!;`           |
| FromStage    | Navigation property | References source stage.                                                         | None                                  | `public virtual ReviewStage FromStage { get; set; } = null!;` |
| ToStage      | Navigation property | References target stage (nullable).                                              | None                                  | `public virtual ReviewStage? ToStage { get; set; }`           |
| Reviews      | Navigation property | One-to-many relationship (one transition rule can have multiple review records). | None                                  | `public virtual ICollection<Reviews> Reviews { get; set; }`   |

## 9. TodoLists (Todo Items Table)

| Field Name            | Type                | Description                                                       | Index Recommendation          | C# Model Mapping                                                |
| --------------------- | ------------------- | ----------------------------------------------------------------- | ----------------------------- | --------------------------------------------------------------- |
| TodoListId            | INT                 | Primary key, uniquely identifies each todo item (auto-increment). | Primary key index (automatic) | `public int TodoListId { get; set; }`                         |
| TemplateId            | INT                 | Foreign key, references used review template (nullable).          | Foreign key index             | `public int? TemplateId { get; set; }`                        |
| CurrentStageId        | INT                 | Foreign key, references current review stage (nullable).          | Foreign key index             | `public int? CurrentStageId { get; set; }`                    |
| Title                 | VARCHAR(255)        | Todo item title.                                                  | None                          | `public string Title { get; set; } = null!;`                  |
| Status                | VARCHAR(50)         | Item current status.                                              | **Single column index** | `public string Status { get; set; } = null!;`                 |
| CreatedByUserId       | INT                 | Foreign key, references creating user.                            | Foreign key index             | `public int CreatedByUserId { get; set; }`                    |
| CurrentReviewerUserId | INT                 | Foreign key, references current reviewer (nullable).              | Foreign key index             | `public int? CurrentReviewerUserId { get; set; }`             |
| CreatedAt             | DATETIME            | Item creation time, defaults to current time.                     | None                          | `public DateTime CreatedAt { get; set; }`                     |
| ReviewTemplate        | Navigation property | References review template (nullable).                            | None                          | `public virtual ReviewTemplate? ReviewTemplate { get; set; }` |
| CurrentStage          | Navigation property | References current review stage (nullable).                       | None                          | `public virtual ReviewStage? CurrentStage { get; set; }`      |
| CreatedByUser         | Navigation property | References creator user.                                          | None                          | `public virtual Users CreatedByUser { get; set; } = null!;`   |
| CurrentReviewerUser   | Navigation property | References current reviewer (nullable).                           | None                          | `public virtual Users? CurrentReviewerUser { get; set; }`     |

## 10. Reviews (Review Records Table)

| Field Name     | Type                | Description                                                           | Index Recommendation          | C# Model Mapping                                             |
| -------------- | ------------------- | --------------------------------------------------------------------- | ----------------------------- | ------------------------------------------------------------ |
| ReviewId       | INT                 | Primary key, uniquely identifies each review record (auto-increment). | Primary key index (automatic) | `public int ReviewId { get; set; }`                        |
| TodoId         | INT                 | Foreign key, references reviewed todo item.                           | Foreign key index             | `public int TodoId { get; set; }`                          |
| ReviewerUserId | INT                 | Foreign key, references user who performed review.                    | Foreign key index             | `public int ReviewerUserId { get; set; }`                  |
| Action         | VARCHAR(50)         | Review action type, e.g., "approve".                                  | None                          | `public string Action { get; set; } = null!;`              |
| ReviewedAt     | DATETIME            | Review execution time, defaults to current time.                      | Index                         | `public DateTime ReviewedAt { get; set; }`                 |
| Comment        | VARCHAR(500)        | Review comment or note, nullable.                                     | None                          | `public string? Comment { get; set; }`                     |
| PreviousStatus | VARCHAR(50)         | Status before review, nullable.                                       | None                          | `public string? PreviousStatus { get; set; }`              |
| NewStatus      | VARCHAR(50)         | Status after review, nullable.                                        | None                          | `public string? NewStatus { get; set; }`                   |
| StageId        | INT                 | Foreign key, references review stage (nullable).                      | Foreign key index             | `public int? StageId { get; set; }`                        |
| ReviewStage    | Navigation property | References review stage (nullable).                                   | None                          | `public virtual ReviewStage? ReviewStage { get; set; }`    |
| Todo           | Navigation property | References todo item.                                                 | None                          | `public virtual TodoLists Todo { get; set; } = null!;`     |
| ReviewerUser   | Navigation property | References reviewer user.                                             | None                          | `public virtual Users ReviewerUser { get; set; } = null!;` |

### Default Data

## 1. Roles (Role Table)

| RoleId | RoleName        |
| ------ | --------------- |
| 1      | Employee        |
| 2      | Senior Employee |
| 3      | Manager         |
| 4      | Administrator   |

## 2. Permissions (Permission Table)

| PermissionId | PermissionName     |
| ------------ | ------------------ |
| 1            | todo_create        |
| 2            | todo_review_level1 |
| 3            | todo_review_level2 |
| 4            | admin_manage       |

## 3. Roles_Permissions (Role-Permission Association Table)

| RoleId | PermissionId |
| ------ | ------------ |
| 1      | 1            |
| 2      | 2            |
| 3      | 3            |
| 4      | 4            |

## 4. Users (User Table)

| UserId | CreatedAt  |
| ------ | ---------- |
| 101    | 2024-01-01 |
| 102    | 2024-01-01 |
| 103    | 2024-01-01 |
| 104    | 2024-01-01 |
| 105    | 2024-01-01 |
| 106    | 2024-01-01 |

## 5. Users_Roles (User-Role Association Table)

| UserId | RoleId |
| ------ | ------ |
| 101    | 1      |
| 102    | 2      |
| 103    | 2      |
| 104    | 3      |
| 105    | 3      |
| 106    | 4      |

---

## 6. ReviewTemplates (Review Template Table)

| TemplateId | TemplateName              | Description                                                            | IsActive | CreatedAt  | CreatedByUserId |
| ---------- | ------------------------- | ---------------------------------------------------------------------- | -------- | ---------- | --------------- |
| 1          | Standard Two-Level Review | Standard two-level review process, includes level 1 and level 2 review | true     | 2024-01-01 | 106             |

## 7. ReviewStages (Review Stage Table)

| StageId | TemplateId | StageName      | StageOrder | RequiredRoleId | SpecificReviewerUserId |
| ------- | ---------- | -------------- | ---------- | -------------- | ---------------------- |
| 1       | 1          | Level 1 Review | 1          | 2              | 102                    |
| 2       | 1          | Level 2 Review | 2          | 3              | null                   |

## 8. StageTransitions (Stage Transition Rules Table)

| TransitionId | StageId | ActionName | NextStageId | ResultStatus          | ReturnType        | ReturnTarget | FallbackType |
| ------------ | ------- | ---------- | ----------- | --------------------- | ----------------- | ------------ | ------------ |
| 1            | 1       | approve    | 2           | pending_review_level2 | NULL              | NULL         | NULL         |
| 2            | 1       | return     | NULL        | returned_to_creator   | previous_reviewer | NULL         | creator      |
| 3            | 1       | reject     | NULL        | rejected              | NULL              | NULL         | NULL         |
| 4            | 2       | approve    | NULL        | approved              | NULL              | NULL         | NULL         |
| 5            | 2       | return     | NULL        | returned_to_level1    | previous_reviewer | NULL         | creator      |
| 6            | 2       | reject     | NULL        | rejected              | NULL              | NULL         | NULL         |



# RBAC + Review  github readme 

一個完整的基於角色權限控制（RBAC）的審核流程管理系統，結合權限管理和多層級審核流程。

## 專案概述

這是一個企業級審核流程管理系統，提供：

- 完整的 RBAC 權限管理：使用者、角色、權限的三層級權限控制
- 靈活的審核流程配置：可自定義多階段審核模板
- 狀態機驅動的審核邏輯：基於 StageTransitions 表的狀態轉換規則
- 完整的審核歷史追蹤：所有審核動作都有完整記錄

### 核心功能

1. 權限管理：CRUD 操作權限與角色
2. 使用者管理：使用者建立與角色分配
3. 審核模板管理：自定義審核流程階段
4. 待辦事項審核：多階段審核流程執行
5. 審核歷史查詢：完整審核歷程追蹤

## 設定分層結構

```
Common                                           
Controller                                       
Repositories                                    
Services                                         
UnitTest  
```

### 各層級專案類型選擇

```
Common 層：選擇「類別庫」
Repositories 層：選擇「類別庫」
Services 層：選擇「類別庫」
Controller 層：選擇「ASP.NET Core Web 應用程式」或「Web API」
UnitTest 層：選擇「單元測試專案」
```

### 技術棧

- 後端框架：ASP.NET Core 8.0+
- 資料庫：SQL Server / Entity Framework Core
- API 設計：RESTful API
- 測試框架：xUnit
- 依賴注入：內建 DI 容器

## 資料庫設計

### 核心實體關係圖

```
┌─────────┐     ┌──────────────┐     ┌──────────────┐
│  Users  │─────│ Users_Roles  │─────│    Roles     │
└─────────┘     └──────────────┘     └──────────────┘
     │                                    │
     │                                    │
┌─────────┐                       ┌──────────────┐
│TodoLists│                       │ Permissions  │
└─────────┘                       └──────────────┘
     │                                    │
     │                                    │
┌─────────┐     ┌──────────────┐     ┌───────────────┐
│ Reviews │─────│ReviewStages  │─────│ReviewTemplates│
└─────────┘     └──────────────┘     └───────────────┘
                      │
                      │
                ┌────────────────┐
                │StageTransitions│
                └────────────────┘
```

## 快速開始

### 環境需求

- .NET 8.0 SDK 或更高版本
- SQL Server 2016+ 或 SQL Server Express
- Visual Studio 2022 或 VS Code

### 安裝步驟

1. 克隆專案
   git clone https://github.com/lauchiwai/rbac_review.git
   cd rbac-reviews
2. 還原 NuGet 套件
   dotnet restore
3. 設定資料庫連線
   編輯 appsettings.json：
   {
   "ConnectionStrings": {
   "DefaultConnection": "Server=localhost;Database=RbacReviewsDB;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   }
4. 執行資料庫遷移
   建立遷移：
   dotnet ef migrations add InitialCreate --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

   更新資料庫：
   dotnet ef database update --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context
5. 執行應用程式
   dotnet run --project .\rbac_reviews\rbac_reviews.csproj
6. 測試 API
   Swagger UI: https://localhost:5001/swagger
   Postman 集合：匯入範例

## 資料庫遷移指令

### 建立新遷移

dotnet ef migrations add [描述性名稱] --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

### 更新資料庫

dotnet ef database update --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

### 常見遷移操作

建立初始遷移：
dotnet ef migrations add InitialCreate --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

更新到特定遷移：
dotnet ef database update 20240101000000_InitialCreate --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

移除最近遷移：
dotnet ef migrations remove --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

## 整合測試

## 審核流程示例

### 多次退回流程

建立 → 一級批准 → 二級退回 → 一級退回 → 創建者補充 → 重新提交 → 一級批准 → 二級批准 → 完成

# 審核前設定

## API 1: 創建二級審核模板

| 欄位         | 值                                                                                                                                      | 說明                            |
| ------------ | --------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------- |
| HTTP Method  | POST                                                                                                                                    |                                 |
| URL          | /api/ReviewTemplate/CreateLevel2Review                                                                                                  |                                 |
| Content-Type | application/json                                                                                                                        |                                 |
| Request Body | `{ "userId": 106, "templateName": "專案審核流程", "description": "標準的二級審核流程，包含一級和二級審核", "level1ReviewerId": 102 }` | 管理員(User106)創建二級審核模板 |

## API 2: 創建待辦事項

| 欄位         | 值                                                            | 說明                      |
| ------------ | ------------------------------------------------------------- | ------------------------- |
| HTTP Method  | POST                                                          |                           |
| URL          | /api/Todo/CreateTodo                                          |                           |
| Content-Type | application/json                                              |                           |
| Request Body | `{ "userId": 101, "title": "Q1專案報告", "templateId": 1 }` | 員工(User101)創建待辦事項 |

# 審核事項

## 步驟1: User102查看初始待審核事項

| 欄位         | 值                                | 說明                  |
| ------------ | --------------------------------- | --------------------- |
| HTTP Method  | GET                               |                       |
| URL          | /api/Review/GetPendingReviews/102 |                       |
| Content-Type | 無                                |                       |
| Request Body | 無                                | User102查看待審核事項 |

## 步驟2: User102批准待辦事項4

| 欄位         | 值                                                                                           | 說明                                             |
| ------------ | -------------------------------------------------------------------------------------------- | ------------------------------------------------ |
| HTTP Method  | POST                                                                                         |                                                  |
| URL          | /api/Review/ExecuteApproveAction                                                             |                                                  |
| Content-Type | application/json                                                                             |                                                  |
| Request Body | `{ "userId": 102, "todoId": 1, "comment": "初審通過，請二級審核", "nextReviewerId": 104 }` | User102批准待辦事項，指定下一階段審核者為User104 |

## 步驟3: User104查看待審核事項

| 欄位         | 值                                | 說明                  |
| ------------ | --------------------------------- | --------------------- |
| HTTP Method  | GET                               |                       |
| URL          | /api/Review/GetPendingReviews/104 |                       |
| Content-Type | 無                                |                       |
| Request Body | 無                                | User104查看待審核事項 |

## 步驟4: User104退回待辦事項4

| 欄位         | 值                                                                      | 說明                          |
| ------------ | ----------------------------------------------------------------------- | ----------------------------- |
| HTTP Method  | POST                                                                    |                               |
| URL          | /api/Review/ExecuteReturnAction                                         |                               |
| Content-Type | application/json                                                        |                               |
| Request Body | `{ "userId": 104, "todoId": 1, "comment": "專案時程需要更詳細規劃" }` | User104退回待辦事項至一級審核 |

## 步驟5: User102查看待審核事項（退回至一級審核狀態）

| 欄位         | 值                                | 說明                        |
| ------------ | --------------------------------- | --------------------------- |
| HTTP Method  | GET                               |                             |
| URL          | /api/Review/GetPendingReviews/102 |                             |
| Content-Type | 無                                |                             |
| Request Body | 無                                | User102查看被退回的待辦事項 |

## 步驟6: User102從退回至一級審核階段退回待辦事項

| 欄位         | 值                                | 說明                        |
| ------------ | --------------------------------- | --------------------------- |
| HTTP Method  | GET                               |                             |
| URL          | /api/Review/GetPendingReviews/101 |                             |
| Content-Type | 無                                |                             |
| Request Body | 無                                | User101查看被退回的待辦事項 |

## 步驟7: User101查看待審核事項（退回至創建者狀態）

| 欄位         | 值                                | 說明                        |
| ------------ | --------------------------------- | --------------------------- |
| HTTP Method  | GET                               |                             |
| URL          | /api/Review/GetPendingReviews/101 |                             |
| Content-Type | 無                                |                             |
| Request Body | 無                                | User101查看被退回的待辦事項 |

## 步驟8: User101重新提交待辦事項

| 欄位         | 值                                                                            | 說明                    |
| ------------ | ----------------------------------------------------------------------------- | ----------------------- |
| HTTP Method  | POST                                                                          |                         |
| URL          | /api/Review/ExecuteResubmitAction                                             |                         |
| Content-Type | application/json                                                              |                         |
| Request Body | `{ "userId": 101, "todoId": 1, "comment": "已補充詳細預算明細和時程規劃" }` | User101重新提交待辦事項 |

## 步驟9: User102查看待審核事項（重新提交後的一級審核）

| 欄位         | 值                                | 說明                          |
| ------------ | --------------------------------- | ----------------------------- |
| HTTP Method  | GET                               |                               |
| URL          | /api/Review/GetPendingReviews/102 |                               |
| Content-Type | 無                                |                               |
| Request Body | 無                                | User102查看重新提交的待辦事項 |

## 步驟10: User102批准待辦事項

| 欄位         | 值                                                                              | 說明                |
| ------------ | ------------------------------------------------------------------------------- | ------------------- |
| HTTP Method  | POST                                                                            |                     |
| URL          | /api/Review/ExecuteApproveAction                                                |                     |
| Content-Type | application/json                                                                |                     |
| Request Body | `{ "userId": 102, "todoId": 1, "comment": "補充資料完整，同意提交二級審核" }` | User102批准待辦事項 |

## 步驟11: User104查看待審核事項（再次進入二級審核）

| 欄位         | 值                                | 說明                  |
| ------------ | --------------------------------- | --------------------- |
| HTTP Method  | GET                               |                       |
| URL          | /api/Review/GetPendingReviews/104 |                       |
| Content-Type | 無                                |                       |
| Request Body | 無                                | User104查看待審核事項 |

## 步驟12: User104批准待辦事項（完成）

| 欄位         | 值                                                                      | 說明                              |
| ------------ | ----------------------------------------------------------------------- | --------------------------------- |
| HTTP Method  | POST                                                                    |                                   |
| URL          | /api/Review/ExecuteApproveAction                                        |                                   |
| Content-Type | application/json                                                        |                                   |
| Request Body | `{ "userId": 104, "todoId": 1, "comment": "專案規劃完整，批准執行" }` | User104批准待辦事項，完成審核流程 |

### 資料表結構

# table

## 1. Roles（角色表）

| 欄位名稱          | 類型         | 功能描述                                    | 索引建議                     | C# 模型對應                                                                       |
| ----------------- | ------------ | ------------------------------------------- | ---------------------------- | --------------------------------------------------------------------------------- |
| RoleId            | INT          | 主鍵，唯一識別每個角色（自動增量）。        | 主鍵索引（自動）             | `public int RoleId { get; set; }`                                               |
| RoleName          | VARCHAR(255) | 角色名稱，例如 "員工"、"資深員工"、"主管"。 | **唯一索引（UNIQUE）** | `public string RoleName { get; set; } = null!;`                                 |
| Roles_Permissions | 導航屬性     | 一對多關係（一個角色可擁有多個權限）。      | 無                           | `public virtual ICollection<Roles_Permissions> Roles_Permissions { get; set; }` |
| Users_Roles       | 導航屬性     | 一對多關係（一個角色可被多個用戶擁有）。    | 無                           | `public virtual ICollection<Users_Roles> Users_Roles { get; set; }`             |

## 2. Permissions（權限表）

| 欄位名稱          | 類型         | 功能描述                                             | 索引建議                     | C# 模型對應                                                                       |
| ----------------- | ------------ | ---------------------------------------------------- | ---------------------------- | --------------------------------------------------------------------------------- |
| PermissionId      | INT          | 主鍵，唯一識別每個權限（自動增量）。                 | 主鍵索引（自動）             | `public int PermissionId { get; set; }`                                         |
| PermissionName    | VARCHAR(255) | 權限名稱，例如 "todo_create"、"todo_review_level1"。 | **唯一索引（UNIQUE）** | `public string PermissionName { get; set; } = null!;`                           |
| Roles_Permissions | 導航屬性     | 一對多關係（一個權限可被多個角色擁有）。             | 無                           | `public virtual ICollection<Roles_Permissions> Roles_Permissions { get; set; }` |

## 3. Roles_Permissions（角色權限關聯表）

| 欄位名稱     | 類型     | 功能描述                                               | 索引建議                       | C# 模型對應                                                      |
| ------------ | -------- | ------------------------------------------------------ | ------------------------------ | ---------------------------------------------------------------- |
| RoleId       | INT      | 外鍵，關聯到 roles 表的 RoleId，表示角色。             | **外鍵索引**（複合主鍵） | `public int RoleId { get; set; }`                              |
| PermissionId | INT      | 外鍵，關聯到 permissions 表的 PermissionId，表示權限。 | **外鍵索引**（複合主鍵） | `public int PermissionId { get; set; }`                        |
| Role         | 導航屬性 | 關聯到 Roles 模型。                                    | 無                             | `public virtual Roles Role { get; set; } = null!;`             |
| Permission   | 導航屬性 | 關聯到 Permissions 模型。                              | 無                             | `public virtual Permissions Permission { get; set; } = null!;` |

**複合主鍵：** (RoleId, PermissionId)

## 4. Users（用戶表）

| 欄位名稱    | 類型     | 功能描述                               | 索引建議         | C# 模型對應                                                           |
| ----------- | -------- | -------------------------------------- | ---------------- | --------------------------------------------------------------------- |
| UserId      | INT      | 主鍵，唯一識別每個用戶（自動增量）。   | 主鍵索引（自動） | `public int UserId { get; set; }`                                   |
| CreatedAt   | DATETIME | 用戶的創建時間，預設為當前時間。       | 無               | `public DateTime CreatedAt { get; set; }`                           |
| Users_Roles | 導航屬性 | 一對多關係（一個用戶可擁有多個角色）。 | 無               | `public virtual ICollection<Users_Roles> Users_Roles { get; set; }` |

## 5. Users_Roles（用戶角色關聯表）

| 欄位名稱 | 類型     | 功能描述                                   | 索引建議                       | C# 模型對應                                          |
| -------- | -------- | ------------------------------------------ | ------------------------------ | ---------------------------------------------------- |
| UserId   | INT      | 外鍵，關聯到 users 表的 UserId，表示用戶。 | **外鍵索引**（複合主鍵） | `public int UserId { get; set; }`                  |
| RoleId   | INT      | 外鍵，關聯到 roles 表的 RoleId，表示角色。 | **外鍵索引**（複合主鍵） | `public int RoleId { get; set; }`                  |
| User     | 導航屬性 | 關聯到 Users 模型。                        | 無                             | `public virtual Users User { get; set; } = null!;` |
| Role     | 導航屬性 | 關聯到 Roles 模型。                        | 無                             | `public virtual Roles Role { get; set; } = null!;` |

**複合主鍵：** (UserId, RoleId)

## 6. ReviewTemplates（審核模板表）

| 欄位名稱        | 類型         | 功能描述                                     | 索引建議                     | C# 模型對應                                                            |
| --------------- | ------------ | -------------------------------------------- | ---------------------------- | ---------------------------------------------------------------------- |
| TemplateId      | INT          | 主鍵，唯一識別每個模板（自動增量）。         | 主鍵索引（自動）             | `public int TemplateId { get; set; }`                                |
| TemplateName    | VARCHAR(255) | 模板名稱，例如 "標準兩級審核"。              | **唯一索引（UNIQUE）** | `public string TemplateName { get; set; } = null!;`                  |
| Description     | VARCHAR(500) | 模板描述。                                   | 無                           | `public string? Description { get; set; }`                           |
| IsActive        | BOOLEAN      | 是否啟用（預設 true）。                      | 索引                         | `public bool IsActive { get; set; } = true`                          |
| CreatedAt       | DATETIME     | 創建時間（預設當前時間）。                   | 無                           | `public DateTime CreatedAt { get; set; }`                            |
| CreatedByUserId | INT          | 創建者用戶ID（可為空）。                     | 外鍵索引                     | `public int? CreatedByUserId { get; set; }`                          |
| CreatedByUser   | 導航屬性     | 關聯到創建者用戶。                           | 無                           | `public virtual Users? CreatedByUser { get; set; }`                  |
| ReviewStages    | 導航屬性     | 一對多關係（一個模板可有多個審核階段）。     | 無                           | `public virtual ICollection<ReviewStage> ReviewStages { get; set; }` |
| TodoLists       | 導航屬性     | 一對多關係（一個模板可被多個待辦事項使用）。 | 無                           | `public virtual ICollection<TodoLists> TodoLists { get; set; }`      |

## 7. ReviewStages（審核階段表）

| 欄位名稱               | 類型         | 功能描述                                             | 索引建議                           | C# 模型對應                                                                        |
| ---------------------- | ------------ | ---------------------------------------------------- | ---------------------------------- | ---------------------------------------------------------------------------------- |
| StageId                | INT          | 主鍵，唯一識別每個階段（自動增量）。                 | 主鍵索引（自動）                   | `public int StageId { get; set; }`                                               |
| TemplateId             | INT          | 外鍵，關聯到審核模板。                               | 外鍵索引                           | `public int TemplateId { get; set; }`                                            |
| StageName              | VARCHAR(255) | 階段名稱，例如 "一級審核"。                          | 無                                 | `public string StageName { get; set; } = null!;`                                 |
| StageOrder             | INT          | 階段順序（從1開始）。                                | 複合索引（TemplateId, StageOrder） | `public int StageOrder { get; set; }`                                            |
| RequiredRoleId         | INT          | 需要的角色ID。                                       | 外鍵索引                           | `public int RequiredRoleId { get; set; }`                                        |
| SpecificReviewerUserId | INT          | 特定審核人用戶ID。                                   | 外鍵索引                           | `public int SpecificReviewerUserId { get; set; }`                                |
| ReviewTemplate         | 導航屬性     | 關聯到審核模板。                                     | 無                                 | `public virtual ReviewTemplate ReviewTemplate { get; set; } = null!;`            |
| RequiredRole           | 導航屬性     | 關聯到需要的角色。                                   | 無                                 | `public virtual Roles RequiredRole { get; set; } = null!;`                       |
| SpecificReviewerUser   | 導航屬性     | 關聯到特定審核人用戶。                               | 無                                 | `public virtual Users SpecificReviewerUser { get; set; } = null!;`               |
| TodoLists              | 導航屬性     | 一對多關係（一個階段可被多個待辦事項作為當前階段）。 | 無                                 | `public virtual ICollection<TodoLists> TodoLists { get; set; }`                  |
| FromStageTransitions   | 導航屬性     | 一對多關係（作為來源階段的轉換規則）。               | 無                                 | `public virtual ICollection<StageTransition> FromStageTransitions { get; set; }` |
| ToStageTransitions     | 導航屬性     | 一對多關係（作為目標階段的轉換規則）。               | 無                                 | `public virtual ICollection<StageTransition> ToStageTransitions { get; set; }`   |
| Reviews                | 導航屬性     | 一對多關係（一個階段可有多個審核記錄）。             | 無                                 | `public virtual ICollection<Reviews> Reviews { get; set; }`                      |

## 8. StageTransitions（階段轉換規則表）

| 欄位名稱     | 類型        | 功能描述                                     | 索引建議                        | C# 模型對應                                                     |
| ------------ | ----------- | -------------------------------------------- | ------------------------------- | --------------------------------------------------------------- |
| TransitionId | INT         | 主鍵，唯一識別每個轉換規則（自動增量）。     | 主鍵索引（自動）                | `public int TransitionId { get; set; }`                       |
| StageId      | INT         | 外鍵，關聯到來源階段。                       | 外鍵索引                        | `public int StageId { get; set; }`                            |
| ActionName   | VARCHAR(50) | 動作名稱，例如 "approve"。                   | 複合索引（StageId, ActionName） | `public string ActionName { get; set; } = null!;`             |
| NextStageId  | INT         | 目標階段ID（可為空）。                       | 外鍵索引                        | `public int? NextStageId { get; set; }`                       |
| ResultStatus | VARCHAR(50) | 結果狀態。                                   | 無                              | `public string ResultStatus { get; set; } = null!;`           |
| FromStage    | 導航屬性    | 關聯到來源階段。                             | 無                              | `public virtual ReviewStage FromStage { get; set; } = null!;` |
| ToStage      | 導航屬性    | 關聯到目標階段（可為空）。                   | 無                              | `public virtual ReviewStage? ToStage { get; set; }`           |
| Reviews      | 導航屬性    | 一對多關係（一個轉換規則可有多個審核記錄）。 | 無                              | `public virtual ICollection<Reviews> Reviews { get; set; }`   |

## 9. TodoLists（待辦事項表）

| 欄位名稱              | 類型         | 功能描述                                 | 索引建議             | C# 模型對應                                                     |
| --------------------- | ------------ | ---------------------------------------- | -------------------- | --------------------------------------------------------------- |
| TodoListId            | INT          | 主鍵，唯一識別每個待辦事項（自動增量）。 | 主鍵索引（自動）     | `public int TodoListId { get; set; }`                         |
| TemplateId            | INT          | 外鍵，關聯到使用的審核模板（可為空）。   | 外鍵索引             | `public int? TemplateId { get; set; }`                        |
| CurrentStageId        | INT          | 外鍵，關聯到當前審核階段（可為空）。     | 外鍵索引             | `public int? CurrentStageId { get; set; }`                    |
| Title                 | VARCHAR(255) | 待辦事項的標題。                         | 無                   | `public string Title { get; set; } = null!;`                  |
| Status                | VARCHAR(50)  | 事項的當前狀態。                         | **單欄位索引** | `public string Status { get; set; } = null!;`                 |
| CreatedByUserId       | INT          | 外鍵，關聯到創建用戶。                   | 外鍵索引             | `public int CreatedByUserId { get; set; }`                    |
| CurrentReviewerUserId | INT          | 外鍵，關聯到當前審核人（可為空）。       | 外鍵索引             | `public int? CurrentReviewerUserId { get; set; }`             |
| CreatedAt             | DATETIME     | 事項的創建時間，預設為當前時間。         | 無                   | `public DateTime CreatedAt { get; set; }`                     |
| ReviewTemplate        | 導航屬性     | 關聯到審核模板（可為空）。               | 無                   | `public virtual ReviewTemplate? ReviewTemplate { get; set; }` |
| CurrentStage          | 導航屬性     | 關聯到當前審核階段（可為空）。           | 無                   | `public virtual ReviewStage? CurrentStage { get; set; }`      |
| CreatedByUser         | 導航屬性     | 關聯到創建者用戶。                       | 無                   | `public virtual Users CreatedByUser { get; set; } = null!;`   |
| CurrentReviewerUser   | 導航屬性     | 關聯到當前審核人（可為空）。             | 無                   | `public virtual Users? CurrentReviewerUser { get; set; }`     |

## 10. Reviews（審核記錄表）

| 欄位名稱       | 類型         | 功能描述                                 | 索引建議         | C# 模型對應                                                  |
| -------------- | ------------ | ---------------------------------------- | ---------------- | ------------------------------------------------------------ |
| ReviewId       | INT          | 主鍵，唯一識別每筆審核記錄（自動增量）。 | 主鍵索引（自動） | `public int ReviewId { get; set; }`                        |
| TodoId         | INT          | 外鍵，關聯到被審核的待辦事項。           | 外鍵索引         | `public int TodoId { get; set; }`                          |
| ReviewerUserId | INT          | 外鍵，關聯到執行審核的用戶。             | 外鍵索引         | `public int ReviewerUserId { get; set; }`                  |
| Action         | VARCHAR(50)  | 審核操作類型，例如 "approve"。           | 無               | `public string Action { get; set; } = null!;`              |
| ReviewedAt     | DATETIME     | 審核執行時間，預設為當前時間。           | 索引             | `public DateTime ReviewedAt { get; set; }`                 |
| Comment        | VARCHAR(500) | 審核評論或備註，可為空。                 | 無               | `public string? Comment { get; set; }`                     |
| PreviousStatus | VARCHAR(50)  | 審核前的狀態，可為空。                   | 無               | `public string? PreviousStatus { get; set; }`              |
| NewStatus      | VARCHAR(50)  | 審核後的狀態，可為空。                   | 無               | `public string? NewStatus { get; set; }`                   |
| StageId        | INT          | 外鍵，關聯到審核階段（可為空）。         | 外鍵索引         | `public int? StageId { get; set; }`                        |
| ReviewStage    | 導航屬性     | 關聯到審核階段（可為空）。               | 無               | `public virtual ReviewStage? ReviewStage { get; set; }`    |
| Todo           | 導航屬性     | 關聯到待辦事項。                         | 無               | `public virtual TodoLists Todo { get; set; } = null!;`     |
| ReviewerUser   | 導航屬性     | 關聯到審核人用戶。                       | 無               | `public virtual Users ReviewerUser { get; set; } = null!;` |

### 預設資料

## 1. Roles（角色表）

| RoleId | RoleName |
| ------ | -------- |
| 1      | 員工     |
| 2      | 資深員工 |
| 3      | 主管     |
| 4      | 管理員   |

## 2. Permissions（權限表）

| PermissionId | PermissionName     |
| ------------ | ------------------ |
| 1            | todo_create        |
| 2            | todo_review_level1 |
| 3            | todo_review_level2 |
| 4            | admin_manage       |

## 3. Roles_Permissions（角色權限關聯表）

| RoleId | PermissionId |
| ------ | ------------ |
| 1      | 1            |
| 2      | 2            |
| 3      | 3            |
| 4      | 4            |

## 4. Users（用戶表）

| UserId | CreatedAt  |
| ------ | ---------- |
| 101    | 2024-01-01 |
| 102    | 2024-01-01 |
| 103    | 2024-01-01 |
| 104    | 2024-01-01 |
| 105    | 2024-01-01 |
| 106    | 2024-01-01 |

## 5. Users_Roles（用戶角色關聯表）

| UserId | RoleId |
| ------ | ------ |
| 101    | 1      |
| 102    | 2      |
| 103    | 2      |
| 104    | 3      |
| 105    | 3      |
| 106    | 4      |

---

## 6. ReviewTemplates（審核模板表）

| TemplateId | TemplateName | Description                            | IsActive | CreatedAt  | CreatedByUserId |
| ---------- | ------------ | -------------------------------------- | -------- | ---------- | --------------- |
| 1          | 標準兩級審核 | 標準的兩級審核流程，包含一級和二級審核 | true     | 2024-01-01 | 106             |

## 7. ReviewStages（審核階段表）

| StageId | TemplateId | StageName | StageOrder | RequiredRoleId | SpecificReviewerUserId |
| ------- | ---------- | --------- | ---------- | -------------- | ---------------------- |
| 1       | 1          | 一級審核  | 1          | 2              | 102                    |
| 2       | 1          | 二級審核  | 2          | 3              | null                   |

## 8. StageTransitions（階段轉換規則表）

| TransitionId | StageId | ActionName | NextStageId | ResultStatus          | ReturnType        | ReturnTarget | FallbackType |
| ------------ | ------- | ---------- | ----------- | --------------------- | ----------------- | ------------ | ------------ |
| 1            | 1       | approve    | 2           | pending_review_level2 | NULL              | NULL         | NULL         |
| 2            | 1       | return     | NULL        | returned_to_creator   | previous_reviewer | NULL         | creator      |
| 3            | 1       | reject     | NULL        | rejected              | NULL              | NULL         | NULL         |
| 4            | 2       | approve    | NULL        | approved              | NULL              | NULL         | NULL         |
| 5            | 2       | return     | NULL        | returned_to_level1    | previous_reviewer | NULL         | creator      |
| 6            | 2       | reject     | NULL        | rejected              | NULL              | NULL         | NULL         |
