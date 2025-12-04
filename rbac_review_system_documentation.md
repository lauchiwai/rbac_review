# RBAC + Review System - Complete Documentation

## Detailed API Documentation

### Base URL

https://localhost:{port}/api/

### 1. Permissions Management API (PermissionsController)

Endpoint: api/Permissions/GetAllPermissions
Method: GET
Description: Get all permissions

Endpoint: api/Permissions/GetPermissionById/{id}
Method: GET
Description: Get permission by ID

Endpoint: api/Permissions/CreatePermission
Method: POST
Description: Create new permission
Request Example: {"permissionName": "todo_review_level1"}

Endpoint: api/Permissions/UpdatePermission/{id}
Method: PUT
Description: Update permission
Request Example: {"permissionId": 1, "permissionName": "updated_name"}

Endpoint: api/Permissions/DeletePermission/{id}
Method: DELETE
Description: Delete permission

### 2. Roles Management API (RolesController)

Endpoint: api/Roles/GetAllRoles
Method: GET
Description: Get all roles

Endpoint: api/Roles/CreateRole
Method: POST
Description: Create new role
Request Example: {"roleName": "Manager"}

Endpoint: api/Roles/UpdateRole
Method: PUT
Description: Update role
Request Example: {"roleId": 1, "roleName": "Updated role name"}

Endpoint: api/Roles/DeleteRole/{id}
Method: DELETE
Description: Delete role

### 3. User Management API (UserController)

Endpoint: api/User/CreateUser
Method: POST
Description: Create new user

Endpoint: api/User/DeleteUser/{userId}
Method: DELETE
Description: Delete user

Endpoint: api/User/AssignRoleToUser
Method: POST
Description: Assign role to user
Request Example: {"userId": 101, "roleId": 2}

Endpoint: api/User/RemoveRoleFromUser/users/{userId}/roles/{roleId}
Method: DELETE
Description: Remove role from user

Endpoint: api/User/GetUserRoles/{userId}
Method: GET
Description: Get user roles

### 4. RBAC Association API (RbacController)

Endpoint: api/Rbac/AssignPermissionToRole
Method: POST
Description: Assign permission to role
Request Example: {"roleId": 2, "permissionId": 3}

Endpoint: api/Rbac/RemovePermissionFromRole/roles/{roleId}/permissions/{permissionId}
Method: DELETE
Description: Remove permission from role

Endpoint: api/Rbac/GetRolePermissions/roles/{roleId}
Method: GET
Description: Get role permissions

Endpoint: api/Rbac/GetRolesWithPermission/permissions/{permissionId}
Method: GET
Description: Get roles with specific permission

### 5. Todo Management API (TodoController)

Endpoint: api/Todo/InitializeReviewTemplate
Method: POST
Description: Initialize review template
Request Example: {"userId":106,"templateName":"Standard Two-Level Review","description":"Standard two-level review process including first and second review","stages":[{"stageName":"First Review","stageOrder":1,"requiredRoleId":2,"specificReviewerUserId":102},{"stageName":"Second Review","stageOrder":2,"requiredRoleId":3,"specificReviewerUserId":104},{"stageName":"Return to Creator","stageOrder":3,"requiredRoleId":1,"specificReviewerUserId":null},{"stageName":"Return to First Review","stageOrder":4,"requiredRoleId":2,"specificReviewerUserId":null}]}

Endpoint: api/Todo/SetupStageTransitions
Method: POST
Description: Setup stage transition rules
Request Example: {"userId":106,"templateId":1,"transitionRules":[{"stageId":1,"actionName":"approve","resultStatus":"pending_review_level2","nextStageId":2},{"stageId":1,"actionName":"return","resultStatus":"returned_to_creator","nextStageId":3},{"stageId":1,"actionName":"reject","resultStatus":"rejected","nextStageId":null},{"stageId":2,"actionName":"approve","resultStatus":"approved","nextStageId":null},{"stageId":2,"actionName":"return","resultStatus":"returned_to_level1","nextStageId":4},{"stageId":2,"actionName":"reject","resultStatus":"rejected","nextStageId":null},{"stageId":3,"actionName":"resubmit","resultStatus":"pending_review_level1","nextStageId":1},{"stageId":4,"actionName":"resubmit","resultStatus":"pending_review_level1","nextStageId":1},{"stageId":4,"actionName":"return","resultStatus":"returned_to_creator","nextStageId":3}]}

Endpoint: api/Todo/CreateTodo
Method: POST
Description: Create todo item
Request Example: {"userId": 101, "title": "Q1 Project Report", "templateId": 1}

### 6. Review Process API (ReviewController)

Endpoint: api/Review/GetPendingReviews/{userId}
Method: GET
Description: Get pending reviews

Endpoint: api/Review/GetTodoDetail/users/{userId}/todos/{todoId}
Method: GET
Description: Get todo detail

Endpoint: api/Review/ExecuteReviewAction
Method: POST
Description: Execute review action
Request Example: {"userId": 102, "todoId": 1001, "action": "approve", "comment": "Review passed"}

Endpoint: api/Review/GetReviewHistory/users/{userId}/todos/{todoId}
Method: POST
Description: Get review history
Request Example: {"userId": 101, "todoId": 1001}

## Integration Test Cases

### Review Process Examples

Multiple return process: Create → First approval → Second return → First return → Creator supplement → Resubmit → First approval → Second approval → Complete

### Pre-Review Setup

#### API 1: Initialize Review Template

HTTP Method: POST
URL: /api/todo/initialize-template
Content-Type: application/json
Request Body: {"userId":106,"templateName":"Standard Two-Level Review","description":"Standard two-level review process including first and second review","stages":[{"stageName":"First Review","stageOrder":1,"requiredRoleId":2,"specificReviewerUserId":102},{"stageName":"Second Review","stageOrder":2,"requiredRoleId":3,"specificReviewerUserId":104},{"stageName":"Return to Creator","stageOrder":3,"requiredRoleId":1,"specificReviewerUserId":null},{"stageName":"Return to First Review","stageOrder":4,"requiredRoleId":2,"specificReviewerUserId":null}]}
Description: Admin(User106) creates review template

#### API 2: Setup Stage Transition Rules

HTTP Method: POST
URL: /api/todo/setup-transitions
Content-Type: application/json
Request Body: {"userId":106,"templateId":1,"transitionRules":[{"stageId":1,"actionName":"approve","resultStatus":"pending_review_level2","nextStageId":2},{"stageId":1,"actionName":"return","resultStatus":"returned_to_creator","nextStageId":3},{"stageId":1,"actionName":"reject","resultStatus":"rejected","nextStageId":null},{"stageId":2,"actionName":"approve","resultStatus":"approved","nextStageId":null},{"stageId":2,"actionName":"return","resultStatus":"returned_to_level1","nextStageId":4},{"stageId":2,"actionName":"reject","resultStatus":"rejected","nextStageId":null},{"stageId":3,"actionName":"resubmit","resultStatus":"pending_review_level1","nextStageId":1},{"stageId":4,"actionName":"resubmit","resultStatus":"pending_review_level1","nextStageId":1},{"stageId":4,"actionName":"return","resultStatus":"returned_to_creator","nextStageId":3}]}
Description: Admin sets all transition rules

#### API 3: Create Todo Item

HTTP Method: POST
URL: /api/todo/create
Content-Type: application/json
Request Body: {"userId": 101, "title": "Q1 Project Report", "templateId": 1}
Description: Employee(User101) creates todo item

### Review Process Flow

#### Step 1: User102 views initial pending reviews

HTTP Method: POST
URL: /api/review/pending
Content-Type: application/json
Request Body: {"userId": 102}
Expected Result: Contains detailed information of pending reviews and available actions

#### Step 2: User102 approves todo item

HTTP Method: POST
URL: /api/review/execute
Content-Type: application/json
Request Body: {"userId": 102, "todoId": 4, "action": "approve", "comment": "First review passed"}
Database Verification: Check if todo status changes to pending_review_level2

#### Steps 3-12: Subsequent review steps

Subsequent steps include User104 viewing, returning, User102 re-reviewing, User101 resubmitting, and other complete processes until final approval.

### Expected Results

All API calls should return standard response format with isSuccess: true, and database status should match the expected state for each step.

## Database Structure Details

### Core Entity Relationship Diagram

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

### Table Structure

#### 1. Roles (Roles Table)

Field Name | Type | Description
RoleId | INT | Primary key, uniquely identifies each role (auto-increment)
RoleName | VARCHAR(255) | Role name, e.g., "Employee", "Senior Employee", "Manager"

#### 2. Permissions (Permissions Table)

Field Name | Type | Description
PermissionId | INT | Primary key, uniquely identifies each permission (auto-increment)
PermissionName | VARCHAR(255) | Permission name, e.g., "todo_create", "todo_review_level1"

#### 3. Roles_Permissions (Role-Permission Association Table)

Field Name | Type | Description
RoleId | INT | Foreign key, references RoleId in roles table
PermissionId | INT | Foreign key, references PermissionId in permissions table

#### 4. Users (Users Table)

Field Name | Type | Description
UserId | INT | Primary key, uniquely identifies each user (auto-increment)
CreatedAt | DATETIME | User creation time, defaults to current time

#### 5. Users_Roles (User-Role Association Table)

Field Name | Type | Description
UserId | INT | Foreign key, references UserId in users table
RoleId | INT | Foreign key, references RoleId in roles table

#### 6. ReviewTemplates (Review Templates Table)

Field Name | Type | Description
TemplateId | INT | Primary key, uniquely identifies each template (auto-increment)
TemplateName | VARCHAR(255) | Template name, e.g., "Standard Two-Level Review"
Description | VARCHAR(500) | Template description
IsActive | BOOLEAN | Whether enabled (default true)
CreatedAt | DATETIME | Creation time (default current time)
CreatedByUserId | INT | Creator user ID (nullable)

#### 7. ReviewStages (Review Stages Table)

Field Name | Type | Description
StageId | INT | Primary key, uniquely identifies each stage (auto-increment)
TemplateId | INT | Foreign key, references review template
StageName | VARCHAR(255) | Stage name, e.g., "First Review"
StageOrder | INT | Stage order (starting from 1)
RequiredRoleId | INT | Required role ID
SpecificReviewerUserId | INT | Specific reviewer user ID

#### 8. StageTransitions (Stage Transition Rules Table)

Field Name | Type | Description
TransitionId | INT | Primary key, uniquely identifies each transition rule (auto-increment)
StageId | INT | Foreign key, references source stage
ActionName | VARCHAR(50) | Action name, e.g., "approve"
NextStageId | INT | Target stage ID (nullable)
ResultStatus | VARCHAR(50) | Result status

#### 9. TodoLists (Todo Items Table)

Field Name | Type | Description
TodoListId | INT | Primary key, uniquely identifies each todo item (auto-increment)
TemplateId | INT | Foreign key, references used review template (nullable)
CurrentStageId | INT | Foreign key, references current review stage (nullable)
Title | VARCHAR(255) | Todo item title
Status | VARCHAR(50) | Current status of the item
CreatedByUserId | INT | Foreign key, references creating user
CurrentReviewerUserId | INT | Foreign key, references current reviewer (nullable)
CreatedAt | DATETIME | Creation time, defaults to current time

#### 10. Reviews (Review Records Table)

Field Name | Type | Description
ReviewId | INT | Primary key, uniquely identifies each review record (auto-increment)
TodoId | INT | Foreign key, references reviewed todo item
ReviewerUserId | INT | Foreign key, references reviewing user
Action | VARCHAR(50) | Review action type, e.g., "approve"
ReviewedAt | DATETIME | Review execution time, defaults to current time
Comment | VARCHAR(500) | Review comment or note (nullable)
PreviousStatus | VARCHAR(50) | Status before review (nullable)
NewStatus | VARCHAR(50) | Status after review (nullable)
StageId | INT | Foreign key, references review stage (nullable)

### Default Data

#### 1. Roles (Roles Table)

RoleId | RoleName
1 | Employee
2 | Senior Employee
3 | Manager
4 | Administrator

#### 2. Permissions (Permissions Table)

PermissionId | PermissionName
1 | todo_create
2 | todo_review_level1
3 | todo_review_level2
4 | admin_manage

#### 3. Roles_Permissions (Role-Permission Association Table)

RoleId | PermissionId
1 | 1
2 | 2
3 | 3
4 | 4

#### 4. Users (Users Table)

UserId | CreatedAt
101 | 2024-01-01
102 | 2024-01-01
103 | 2024-01-01
104 | 2024-01-01
105 | 2024-01-01
106 | 2024-01-01

#### 5. Users_Roles (User-Role Association Table)

UserId | RoleId
101 | 1
102 | 2
103 | 2
104 | 3
105 | 3
106 | 4

#### 6. ReviewTemplates (Review Templates Table)

TemplateId | TemplateName | Description | IsActive | CreatedAt | CreatedByUserId
1 | Standard Two-Level Review | Standard two-level review process including first and second review | true | 2024-01-01 | 106

#### 7. ReviewStages (Review Stages Table)

StageId | TemplateId | StageName | StageOrder | RequiredRoleId | SpecificReviewerUserId
1 | 1 | First Review | 1 | 2 | 102
2 | 1 | Second Review | 2 | 3 | 104
3 | 1 | Return to Creator | 3 | 1 | null
4 | 1 | Return to First Review | 4 | 2 | null

#### 8. StageTransitions (Stage Transition Rules Table)

TransitionId | StageId | ActionName | NextStageId | ResultStatus
1 | 1 | approve | 2 | pending_review_level2
2 | 1 | return | 3 | returned_to_creator
3 | 1 | reject | NULL | rejected
4 | 2 | approve | NULL | approved
5 | 2 | return | 4 | returned_to_level1
6 | 2 | reject | NULL | rejected
7 | 3 | resubmit | 1 | pending_review_level1
8 | 4 | resubmit | 1 | pending_review_level1
9 | 4 | return | 3 | returned_to_creator

# RBAC + Review 系統 - 完整文檔

## 詳細 API 文檔

### 基礎 URL

https://localhost:{port}/api/

### 1. 權限管理 API (PermissionsController)

端點：api/Permissions/GetAllPermissions
方法：GET
描述：取得所有權限

端點：api/Permissions/GetPermissionById/{id}
方法：GET
描述：依 ID 取得權限

端點：api/Permissions/CreatePermission
方法：POST
描述：建立新權限
請求範例：{"permissionName": "todo_review_level1"}

端點：api/Permissions/UpdatePermission/{id}
方法：PUT
描述：更新權限
請求範例：{"permissionId": 1, "permissionName": "updated_name"}

端點：api/Permissions/DeletePermission/{id}
方法：DELETE
描述：刪除權限

### 2. 角色管理 API (RolesController)

端點：api/Roles/GetAllRoles
方法：GET
描述：取得所有角色

端點：api/Roles/CreateRole
方法：POST
描述：建立新角色
請求範例：{"roleName": "主管"}

端點：api/Roles/UpdateRole
方法：PUT
描述：更新角色
請求範例：{"roleId": 1, "roleName": "更新角色名"}

端點：api/Roles/DeleteRole/{id}
方法：DELETE
描述：刪除角色

### 3. 使用者管理 API (UserController)

端點：api/User/CreateUser
方法：POST
描述：建立新使用者

端點：api/User/DeleteUser/{userId}
方法：DELETE
描述：刪除使用者

端點：api/User/AssignRoleToUser
方法：POST
描述：分配角色給使用者
請求範例：{"userId": 101, "roleId": 2}

端點：api/User/RemoveRoleFromUser/users/{userId}/roles/{roleId}
方法：DELETE
描述：移除使用者角色

端點：api/User/GetUserRoles/{userId}
方法：GET
描述：取得使用者角色

### 4. RBAC 關聯 API (RbacController)

端點：api/Rbac/AssignPermissionToRole
方法：POST
描述：分配權限給角色
請求範例：{"roleId": 2, "permissionId": 3}

端點：api/Rbac/RemovePermissionFromRole/roles/{roleId}/permissions/{permissionId}
方法：DELETE
描述：移除角色權限

端點：api/Rbac/GetRolePermissions/roles/{roleId}
方法：GET
描述：取得角色權限

端點：api/Rbac/GetRolesWithPermission/permissions/{permissionId}
方法：GET
描述：取得擁有權限的角色

### 5. 待辦事項 API (TodoController)

端點：api/Todo/InitializeReviewTemplate
方法：POST
描述：初始化審核模板
請求範例：{"userId":106,"templateName":"標準兩級審核","description":"標準的兩級審核流程，包含一級和二級審核","stages":[{"stageName":"一級審核","stageOrder":1,"requiredRoleId":2,"specificReviewerUserId":102},{"stageName":"二級審核","stageOrder":2,"requiredRoleId":3,"specificReviewerUserId":104},{"stageName":"退回至創建者","stageOrder":3,"requiredRoleId":1,"specificReviewerUserId":null},{"stageName":"退回至一級審核","stageOrder":4,"requiredRoleId":2,"specificReviewerUserId":null}]}

端點：api/Todo/SetupStageTransitions
方法：POST
描述：設定階段轉換規則
請求範例：{"userId":106,"templateId":1,"transitionRules":[{"stageId":1,"actionName":"approve","resultStatus":"pending_review_level2","nextStageId":2},{"stageId":1,"actionName":"return","resultStatus":"returned_to_creator","nextStageId":3},{"stageId":1,"actionName":"reject","resultStatus":"rejected","nextStageId":null},{"stageId":2,"actionName":"approve","resultStatus":"approved","nextStageId":null},{"stageId":2,"actionName":"return","resultStatus":"returned_to_level1","nextStageId":4},{"stageId":2,"actionName":"reject","resultStatus":"rejected","nextStageId":null},{"stageId":3,"actionName":"resubmit","resultStatus":"pending_review_level1","nextStageId":1},{"stageId":4,"actionName":"resubmit","resultStatus":"pending_review_level1","nextStageId":1},{"stageId":4,"actionName":"return","resultStatus":"returned_to_creator","nextStageId":3}]}

端點：api/Todo/CreateTodo
方法：POST
描述：建立待辦事項
請求範例：{"userId": 101, "title": "Q1專案報告", "templateId": 1}

### 6. 審核流程 API (ReviewController)

端點：api/Review/GetPendingReviews/{userId}
方法：GET
描述：取得待審核事項

端點：api/Review/GetTodoDetail/users/{userId}/todos/{todoId}
方法：GET
描述：取得待辦事項詳情

端點：api/Review/ExecuteReviewAction
方法：POST
描述：執行審核動作
請求範例：{"userId": 102, "todoId": 1001, "action": "approve", "comment": "審核通過"}

端點：api/Review/GetReviewHistory/users/{userId}/todos/{todoId}
方法：POST
描述：取得審核歷史
請求範例：{"userId": 101, "todoId": 1001}

## 整合測試案例

### 審核流程示例

多次退回流程：建立 → 一級批准 → 二級退回 → 一級退回 → 創建者補充 → 重新提交 → 一級批准 → 二級批准 → 完成

### 審核前設定

#### API 1: 初始化審核模板

HTTP Method: POST
URL: /api/todo/initialize-template
Content-Type: application/json
Request Body: {"userId":106,"templateName":"標準兩級審核","description":"標準的兩級審核流程，包含一級和二級審核","stages":[{"stageName":"一級審核","stageOrder":1,"requiredRoleId":2,"specificReviewerUserId":102},{"stageName":"二級審核","stageOrder":2,"requiredRoleId":3,"specificReviewerUserId":104},{"stageName":"退回至創建者","stageOrder":3,"requiredRoleId":1,"specificReviewerUserId":null},{"stageName":"退回至一級審核","stageOrder":4,"requiredRoleId":2,"specificReviewerUserId":null}]}
說明：管理員(User106)創建審核模板

#### API 2: 設定階段轉換規則

HTTP Method: POST
URL: /api/todo/setup-transitions
Content-Type: application/json
Request Body: {"userId":106,"templateId":1,"transitionRules":[{"stageId":1,"actionName":"approve","resultStatus":"pending_review_level2","nextStageId":2},{"stageId":1,"actionName":"return","resultStatus":"returned_to_creator","nextStageId":3},{"stageId":1,"actionName":"reject","resultStatus":"rejected","nextStageId":null},{"stageId":2,"actionName":"approve","resultStatus":"approved","nextStageId":null},{"stageId":2,"actionName":"return","resultStatus":"returned_to_level1","nextStageId":4},{"stageId":2,"actionName":"reject","resultStatus":"rejected","nextStageId":null},{"stageId":3,"actionName":"resubmit","resultStatus":"pending_review_level1","nextStageId":1},{"stageId":4,"actionName":"resubmit","resultStatus":"pending_review_level1","nextStageId":1},{"stageId":4,"actionName":"return","resultStatus":"returned_to_creator","nextStageId":3}]}
說明：管理員設定全部轉換規則

#### API 3: 創建待辦事項

HTTP Method: POST
URL: /api/todo/create
Content-Type: application/json
Request Body: {"userId": 101, "title": "Q1專案報告", "templateId": 1}
說明：員工(User101)創建待辦事項

### 審核事項流程

#### 步驟1: User102查看初始待審核事項

HTTP Method: POST
URL: /api/review/pending
Content-Type: application/json
Request Body: {"userId": 102}
預期結果：包含待審核事項的詳細資訊和可用操作

#### 步驟2: User102批准待辦事項

HTTP Method: POST
URL: /api/review/execute
Content-Type: application/json
Request Body: {"userId": 102, "todoId": 4, "action": "approve", "comment": "一級審核通過"}
數據庫驗證：檢查待辦事項狀態是否變更為 pending_review_level2

#### 步驟3-12: 後續審核步驟

後續步驟包括User104查看、退回、User102重新審核、User101重新提交等完整流程，最終完成審核。

### 預期結果

所有API呼叫應返回包含 isSuccess: true 的標準回應格式，數據庫狀態應與每個步驟的預期狀態相符。

## 資料庫結構詳情

### 核心實體關係圖

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

### 資料表結構

#### 1. Roles（角色表）

欄位名稱 | 類型 | 功能描述
RoleId | INT | 主鍵，唯一識別每個角色（自動增量）
RoleName | VARCHAR(255) | 角色名稱，例如 "員工"、"資深員工"、"主管"

#### 2. Permissions（權限表）

欄位名稱 | 類型 | 功能描述
PermissionId | INT | 主鍵，唯一識別每個權限（自動增量）
PermissionName | VARCHAR(255) | 權限名稱，例如 "todo_create"、"todo_review_level1"

#### 3. Roles_Permissions（角色權限關聯表）

欄位名稱 | 類型 | 功能描述
RoleId | INT | 外鍵，關聯到 roles 表的 RoleId
PermissionId | INT | 外鍵，關聯到 permissions 表的 PermissionId

#### 4. Users（用戶表）

欄位名稱 | 類型 | 功能描述
UserId | INT | 主鍵，唯一識別每個用戶（自動增量）
CreatedAt | DATETIME | 用戶的創建時間，預設為當前時間

#### 5. Users_Roles（用戶角色關聯表）

欄位名稱 | 類型 | 功能描述
UserId | INT | 外鍵，關聯到 users 表的 UserId
RoleId | INT | 外鍵，關聯到 roles 表的 RoleId

#### 6. ReviewTemplates（審核模板表）

欄位名稱 | 類型 | 功能描述
TemplateId | INT | 主鍵，唯一識別每個模板（自動增量）
TemplateName | VARCHAR(255) | 模板名稱，例如 "標準兩級審核"
Description | VARCHAR(500) | 模板描述
IsActive | BOOLEAN | 是否啟用（預設 true）
CreatedAt | DATETIME | 創建時間（預設當前時間）
CreatedByUserId | INT | 創建者用戶ID（可為空）

#### 7. ReviewStages（審核階段表）

欄位名稱 | 類型 | 功能描述
StageId | INT | 主鍵，唯一識別每個階段（自動增量）
TemplateId | INT | 外鍵，關聯到審核模板
StageName | VARCHAR(255) | 階段名稱，例如 "一級審核"
StageOrder | INT | 階段順序（從1開始）
RequiredRoleId | INT | 需要的角色ID
SpecificReviewerUserId | INT | 特定審核人用戶ID

#### 8. StageTransitions（階段轉換規則表）

欄位名稱 | 類型 | 功能描述
TransitionId | INT | 主鍵，唯一識別每個轉換規則（自動增量）
StageId | INT | 外鍵，關聯到來源階段
ActionName | VARCHAR(50) | 動作名稱，例如 "approve"
NextStageId | INT | 目標階段ID（可為空）
ResultStatus | VARCHAR(50) | 結果狀態

#### 9. TodoLists（待辦事項表）

欄位名稱 | 類型 | 功能描述
TodoListId | INT | 主鍵，唯一識別每個待辦事項（自動增量）
TemplateId | INT | 外鍵，關聯到使用的審核模板（可為空）
CurrentStageId | INT | 外鍵，關聯到當前審核階段（可為空）
Title | VARCHAR(255) | 待辦事項的標題
Status | VARCHAR(50) | 事項的當前狀態
CreatedByUserId | INT | 外鍵，關聯到創建用戶
CurrentReviewerUserId | INT | 外鍵，關聯到當前審核人（可為空）
CreatedAt | DATETIME | 事項的創建時間，預設為當前時間

#### 10. Reviews（審核記錄表）

欄位名稱 | 類型 | 功能描述
ReviewId | INT | 主鍵，唯一識別每筆審核記錄（自動增量）
TodoId | INT | 外鍵，關聯到被審核的待辦事項
ReviewerUserId | INT | 外鍵，關聯到執行審核的用戶
Action | VARCHAR(50) | 審核操作類型，例如 "approve"
ReviewedAt | DATETIME | 審核執行時間，預設為當前時間
Comment | VARCHAR(500) | 審核評論或備註，可為空
PreviousStatus | VARCHAR(50) | 審核前的狀態，可為空
NewStatus | VARCHAR(50) | 審核後的狀態，可為空
StageId | INT | 外鍵，關聯到審核階段（可為空）

### 預設資料

#### 1. Roles（角色表）

RoleId | RoleName
1 | 員工
2 | 資深員工
3 | 主管
4 | 管理員

#### 2. Permissions（權限表）

PermissionId | PermissionName
1 | todo_create
2 | todo_review_level1
3 | todo_review_level2
4 | admin_manage

#### 3. Roles_Permissions（角色權限關聯表）

RoleId | PermissionId
1 | 1
2 | 2
3 | 3
4 | 4

#### 4. Users（用戶表）

UserId | CreatedAt
101 | 2024-01-01
102 | 2024-01-01
103 | 2024-01-01
104 | 2024-01-01
105 | 2024-01-01
106 | 2024-01-01

#### 5. Users_Roles（用戶角色關聯表）

UserId | RoleId
101 | 1
102 | 2
103 | 2
104 | 3
105 | 3
106 | 4

#### 6. ReviewTemplates（審核模板表）

TemplateId | TemplateName | Description | IsActive | CreatedAt | CreatedByUserId
1 | 標準兩級審核 | 標準的兩級審核流程，包含一級和二級審核 | true | 2024-01-01 | 106

#### 7. ReviewStages（審核階段表）

StageId | TemplateId | StageName | StageOrder | RequiredRoleId | SpecificReviewerUserId
1 | 1 | 一級審核 | 1 | 2 | 102
2 | 1 | 二級審核 | 2 | 3 | 104
3 | 1 | 退回至創建者 | 3 | 1 | null
4 | 1 | 退回至一級審核 | 4 | 2 | null

#### 8. StageTransitions（階段轉換規則表）

TransitionId | StageId | ActionName | NextStageId | ResultStatus
1 | 1 | approve | 2 | pending_review_level2
2 | 1 | return | 3 | returned_to_creator
3 | 1 | reject | NULL | rejected
4 | 2 | approve | NULL | approved
5 | 2 | return | 4 | returned_to_level1
6 | 2 | reject | NULL | rejected
7 | 3 | resubmit | 1 | pending_review_level1
8 | 4 | resubmit | 1 | pending_review_level1
9 | 4 | return | 3 | returned_to_creator
