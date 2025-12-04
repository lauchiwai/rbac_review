
# RBAC + Review System

A complete Role-Based Access Control (RBAC) review process management system that combines permission management with multi-level review workflows.

## Project Overview

This system provides enterprise-level review process management with complete RBAC permission control, flexible review process configuration, and state-machine-driven review logic.

### Core Features

- Complete RBAC permission management
- User management and role assignment
- Customizable multi-stage review templates
- State-machine-driven review processes
- Complete review history tracking

## System Architecture

### Layered Structure

Common (Class Library)          - DTOs, Models, Shared utilities
Repositories (Class Library)    - Data access layer
Services (Class Library)        - Business logic layer
Controller (Web API)            - API controller layer
UnitTest (Test Project)         - Unit testing

### Technology Stack

- Backend Framework: ASP.NET Core 8.0+
- Database: SQL Server / Entity Framework Core
- API Design: RESTful API
- Testing Framework: xUnit
- Architecture Pattern: Layered architecture

## Core Database Tables

| Table Name        | Description                 | Main Functions                                          |
| ----------------- | --------------------------- | ------------------------------------------------------- |
| Users             | User Table                  | Stores user information                                 |
| Roles             | Role Table                  | Defines system roles                                    |
| Permissions       | Permission Table            | Defines system permissions                              |
| Users_Roles       | User-Role Association       | Many-to-many relationship between users and roles       |
| Roles_Permissions | Role-Permission Association | Many-to-many relationship between roles and permissions |
| ReviewTemplates   | Review Template Table       | Defines review process templates                        |
| ReviewStages      | Review Stage Table          | Various stages of review process                        |
| StageTransitions  | Stage Transition Rules      | Defines state transition rules                          |
| TodoLists         | Todo Items Table            | Stores pending review items                             |
| Reviews           | Review Records Table        | Records all review operation history                    |

## Quick Start

### Requirements

- .NET 8.0 SDK
- SQL Server 2016+
- Visual Studio 2022 or VS Code

### Installation Steps

1. Clone the project
   git clone https://github.com/lauchiwai/rbac_review.git
   cd rbac-reviews
2. Restore packages and configure database
   dotnet restore
   Edit appsettings.json to set database connection
3. Run database migrations
   dotnet ef migrations add InitialCreate --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context
   dotnet ef database update --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context
4. Start the application
   dotnet run --project .\rbac_reviews\rbac_reviews.csproj
5. Test APIs
   Swagger UI: https://localhost:5001/swagger
   Use Postman to test API endpoints

## API Endpoints Overview

### Permissions Management (api/Permissions)

GET /GetAllPermissions - Get all permissions
GET /GetPermissionById/{id} - Get permission by ID
POST /CreatePermission - Create new permission
PUT /UpdatePermission/{id} - Update permission
DELETE /DeletePermission/{id} - Delete permission

### Roles Management (api/Roles)

GET /GetAllRoles - Get all roles
POST /CreateRole - Create new role
PUT /UpdateRole - Update role
DELETE /DeleteRole/{id} - Delete role

### User Management (api/User)

POST /CreateUser - Create new user
DELETE /DeleteUser/{userId} - Delete user
POST /AssignRoleToUser - Assign role to user
DELETE /RemoveRoleFromUser/users/{userId}/roles/{roleId} - Remove role from user
GET /GetUserRoles/{userId} - Get user roles

### RBAC Association Management (api/Rbac)

POST /AssignPermissionToRole - Assign permission to role
DELETE /RemovePermissionFromRole/roles/{roleId}/permissions/{permissionId} - Remove permission from role
GET /GetRolePermissions/roles/{roleId} - Get role permissions
GET /GetRolesWithPermission/permissions/{permissionId} - Get roles with specific permission

### Todo Management (api/Todo)

POST /InitializeReviewTemplate - Initialize review template
POST /SetupStageTransitions - Set up stage transition rules
POST /CreateTodo - Create todo item

### Review Process (api/Review)

GET /GetPendingReviews/{userId} - Get pending reviews
GET /GetTodoDetail/users/{userId}/todos/{todoId} - Get todo item details
POST /ExecuteReviewAction - Execute review action
POST /GetReviewHistory/users/{userId}/todos/{todoId} - Get review history

## Review Process Examples

### Standard Two-Level Review Process

Create todo item → First-level review → Second-level review → Complete

### Return and Re-review Process

Create → First approval → Second return → First re-review → Second approval → Complete

### Complete Test Process (including multiple returns)

See detailed test cases in the rbac_review_system_documentation.md document.

## Database Migration Commands

Create new migration:
dotnet ef migrations add [description] --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

Update database:
dotnet ef database update --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

Remove recent migration:
dotnet ef migrations remove --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

## Default Data

### Roles and Permissions

| Role            | Permissions        | Example Users    |
| --------------- | ------------------ | ---------------- |
| Employee        | todo_create        | User101          |
| Senior Employee | todo_review_level1 | User102, User103 |
| Manager         | todo_review_level2 | User104, User105 |
| Administrator   | admin_manage       | User106          |

### Review Templates

- Template Name: Standard Two-Level Review
- Review Stages: First Review, Second Review, Return to Creator, Return to First Review
- State Transition Rules: Includes actions like approve, return, reject, resubmit

## Development Guide

### Steps for Adding New Features

1. Add DTOs in Common layer
2. Add Entity and Repository in Repositories layer
3. Add Service interface and implementation in Services layer
4. Add API endpoints in Controller layer
5. Add test cases in UnitTest layer

### Coding Standards

- Use async/await for asynchronous operations
- Follow RESTful API design principles
- Implement complete error handling
- Write unit tests covering core logic

## Related Documentation

- Complete System Documentation: rbac_review_system_documentation.md

# RBAC + Review 系統

一個完整的基於角色權限控制（RBAC）的審核流程管理系統，結合權限管理和多層級審核流程。

## 專案概述

這個系統提供企業級審核流程管理，具有完整的RBAC權限控制、靈活的審核流程配置和狀態機驅動的審核邏輯。

### 核心功能

- 完整的RBAC權限管理
- 使用者管理與角色分配
- 可自定義的多階段審核模板
- 狀態機驅動的審核流程
- 完整的審核歷史追蹤

## 系統架構

### 分層結構

Common (類別庫)          - DTOs、Models、共用工具
Repositories (類別庫)    - 資料存取層
Services (類別庫)        - 業務邏輯層
Controller (Web API)     - API控制器層
UnitTest (測試專案)      - 單元測試

### 技術棧

- 後端框架: ASP.NET Core 8.0+
- 資料庫: SQL Server / Entity Framework Core
- API設計: RESTful API
- 測試框架: xUnit
- 架構模式: 分層架構

## 核心資料表

| 表名              | 描述           | 主要功能                 |
| ----------------- | -------------- | ------------------------ |
| Users             | 使用者表       | 存儲使用者資訊           |
| Roles             | 角色表         | 定義系統角色             |
| Permissions       | 權限表         | 定義系統權限             |
| Users_Roles       | 使用者角色關聯 | 使用者與角色的多對多關係 |
| Roles_Permissions | 角色權限關聯   | 角色與權限的多對多關係   |
| ReviewTemplates   | 審核模板表     | 定義審核流程模板         |
| ReviewStages      | 審核階段表     | 審核流程的各個階段       |
| StageTransitions  | 階段轉換規則   | 定義狀態轉換規則         |
| TodoLists         | 待辦事項表     | 存儲待審核事項           |
| Reviews           | 審核記錄表     | 記錄所有審核操作歷史     |

## 快速開始

### 環境需求

- .NET 8.0 SDK
- SQL Server 2016+
- Visual Studio 2022 或 VS Code

### 安裝步驟

1. 克隆專案
   git clone https://github.com/lauchiwai/rbac_review.git
   cd rbac-reviews
2. 還原套件與設定資料庫
   dotnet restore
   編輯 appsettings.json 設定資料庫連線
3. 執行資料庫遷移
   dotnet ef migrations add InitialCreate --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context
   dotnet ef database update --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context
4. 啟動應用程式
   dotnet run --project .\rbac_reviews\rbac_reviews.csproj
5. 測試API
   Swagger UI: https://localhost:5001/swagger
   使用Postman測試API端點

## API 端點總覽

### 權限管理 (api/Permissions)

GET /GetAllPermissions - 取得所有權限
GET /GetPermissionById/{id} - 依ID取得權限
POST /CreatePermission - 建立新權限
PUT /UpdatePermission/{id} - 更新權限
DELETE /DeletePermission/{id} - 刪除權限

### 角色管理 (api/Roles)

GET /GetAllRoles - 取得所有角色
POST /CreateRole - 建立新角色
PUT /UpdateRole - 更新角色
DELETE /DeleteRole/{id} - 刪除角色

### 使用者管理 (api/User)

POST /CreateUser - 建立新使用者
DELETE /DeleteUser/{userId} - 刪除使用者
POST /AssignRoleToUser - 分配角色給使用者
DELETE /RemoveRoleFromUser/users/{userId}/roles/{roleId} - 移除使用者角色
GET /GetUserRoles/{userId} - 取得使用者角色

### RBAC關聯管理 (api/Rbac)

POST /AssignPermissionToRole - 分配權限給角色
DELETE /RemovePermissionFromRole/roles/{roleId}/permissions/{permissionId} - 移除角色權限
GET /GetRolePermissions/roles/{roleId} - 取得角色權限
GET /GetRolesWithPermission/permissions/{permissionId} - 取得擁有權限的角色

### 待辦事項管理 (api/Todo)

POST /InitializeReviewTemplate - 初始化審核模板
POST /SetupStageTransitions - 設定階段轉換規則
POST /CreateTodo - 建立待辦事項

### 審核流程 (api/Review)

GET /GetPendingReviews/{userId} - 取得待審核事項
GET /GetTodoDetail/users/{userId}/todos/{todoId} - 取得待辦事項詳情
POST /ExecuteReviewAction - 執行審核動作
POST /GetReviewHistory/users/{userId}/todos/{todoId} - 取得審核歷史

## 審核流程示例

### 標準兩級審核流程

建立待辦事項 → 一級審核 → 二級審核 → 完成

### 退回重審流程

建立 → 一級批准 → 二級退回 → 一級重新審核 → 二級批准 → 完成

### 完整測試流程（包含多次退回）

詳見 rbac_review_system_documentation.md 文件中的詳細測試案例。

## 資料庫遷移指令

建立新遷移:
dotnet ef migrations add [描述性名稱] --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

更新資料庫:
dotnet ef database update --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

移除最近遷移:
dotnet ef migrations remove --project .\Repositories\Repositories.csproj --startup-project .\rbac_reviews\rbac_reviews.csproj --context Context

## 預設資料

### 角色與權限

| 角色     | 權限               | 示例使用者       |
| -------- | ------------------ | ---------------- |
| 員工     | todo_create        | User101          |
| 資深員工 | todo_review_level1 | User102, User103 |
| 主管     | todo_review_level2 | User104, User105 |
| 管理員   | admin_manage       | User106          |

### 審核模板

- 模板名稱: 標準兩級審核
- 審核階段: 一級審核、二級審核、退回至創建者、退回至一級審核
- 狀態轉換規則: 包含 approve、return、reject、resubmit 等動作

## 開發指南

### 新增功能步驟

1. 在 Common 層新增 DTOs
2. 在 Repositories 層新增 Entity 和 Repository
3. 在 Services 層新增 Service 介面與實作
4. 在 Controller 層新增 API 端點
5. 在 UnitTest 層新增測試案例

### 編碼規範

- 使用 async/await 進行非同步操作
- 遵循 RESTful API 設計原則
- 實作完整的錯誤處理
- 撰寫單元測試覆蓋核心邏輯

## 相關文檔

- 完整系統文檔: rbac_review_system_documentation.md
