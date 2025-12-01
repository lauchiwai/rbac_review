// Services/Helpers/TodoQueryHelper.cs
using Common.DTOs.ReviewTodoLists.Responses;
using Common.DTOs.TodoLists.Responses;
using Common.Enums;
using Common.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.MyRepository;
using Services.Interfaces;
using System.Linq.Expressions;

namespace Services.Helpers
{
    public class TodoQueryHelper
    {
        private readonly IRepository<TodoLists> _todoRepository;
        private readonly IRepository<Reviews> _reviewRepository;
        private readonly IRepository<Users_Roles> _userRoleRepository;
        private readonly IRepository<Roles> _roleRepository;
        private readonly IRbacService _rbacService;

        public TodoQueryHelper(
            IRepository<TodoLists> todoRepository,
            IRepository<Reviews> reviewRepository,
            IRepository<Users_Roles> userRoleRepository,
            IRepository<Roles> roleRepository,
            IRbacService rbacService)
        {
            _todoRepository = todoRepository;
            _reviewRepository = reviewRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _rbacService = rbacService;
        }

        // Core method: One-time fetch of all review todo items
        public async Task<List<TodoWithReviewHistoryViewModel>> GetReviewTodosAsync(
            int currentUserId,
            string status = null)
        {
            // 1. Get all user roles and permissions (one-time fetch)
            var userPermissions = await GetUserPermissionsAsync(currentUserId);

            // 2. Batch fetch todo item IDs that the user has participated in
            var participatedTodoIds = await GetParticipatedTodoIdsAsync(currentUserId);

            // 3. Get eligible todo item IDs
            var eligibleTodoIds = await GetEligibleTodoIdsAsync(
                currentUserId,
                userPermissions,
                participatedTodoIds,
                status);

            if (!eligibleTodoIds.Any())
                return new List<TodoWithReviewHistoryViewModel>();

            // 4. Batch query todo items
            var todos = await _todoRepository.FindAsync(t => eligibleTodoIds.Contains(t.TodoListId));

            // 5. Batch query review records
            var reviews = await _reviewRepository.FindAsync(r => eligibleTodoIds.Contains(r.TodoId));
            var reviewsByTodoId = reviews
                .GroupBy(r => r.TodoId)
                .ToDictionary(g => g.Key, g => g.OrderBy(r => r.ReviewedAt).ToList());

            // 6. Convert to ViewModel
            return MapToViewModel(todos.ToList(), reviewsByTodoId);
        }

        // Helper method: Get todo item IDs that the user has participated in
        private async Task<List<int>> GetParticipatedTodoIdsAsync(int userId)
        {
            var reviews = await _reviewRepository.FindAsync(r => r.ReviewerUserId == userId);
            return reviews.Select(r => r.TodoId).Distinct().ToList();
        }

        // Helper method: Get eligible todo item IDs
        private async Task<List<int>> GetEligibleTodoIdsAsync(
            int currentUserId,
            UserPermissions permissions,
            List<int> participatedTodoIds,
            string status)
        {
            // Use IQueryable for querying
            var query = _todoRepository.GetQueryable();

            // Build conditions
            Expression<Func<TodoLists, bool>> condition = BuildTodoCondition(
                currentUserId, permissions, participatedTodoIds, status);

            // Only get IDs to reduce data transmission
            return await query
                .Where(condition)
                .Select(t => t.TodoListId)
                .ToListAsync();
        }

        // Helper method: Get user permissions
        private async Task<UserPermissions> GetUserPermissionsAsync(int userId)
        {
            var userRoles = await GetUserRolesAsync(userId);
            var permissions = new UserPermissions();

            // Query each permission only once, not once per role
            var tasks = new List<Task>();

            // Check TodoReviewLevel1 permission
            tasks.Add(CheckPermissionAsync(userRoles,
                Permission.TodoReviewLevel1.GetPermissionName(),
                result => permissions.HasReviewLevel1Permission = result));

            // Check TodoReviewLevel2 permission
            tasks.Add(CheckPermissionAsync(userRoles,
                Permission.TodoReviewLevel2.GetPermissionName(),
                result => permissions.HasReviewLevel2Permission = result));

            await Task.WhenAll(tasks);
            return permissions;
        }

        // Helper method: Check permissions
        private async Task CheckPermissionAsync(
            List<Roles> userRoles,
            string permissionName,
            Action<bool> setResult)
        {
            foreach (var role in userRoles)
            {
                var result = await _rbacService.HasPermissionAsync(role.RoleId, permissionName);
                if (result.IsSuccess && result.Data)
                {
                    setResult(true);
                    return;
                }
            }
            setResult(false);
        }

        // Fix: Explicitly convert to List<Roles>
        private async Task<List<Roles>> GetUserRolesAsync(int userId)
        {
            // Use batch query to avoid N+1
            var userRoles = await _userRoleRepository.FindAsync(ur => ur.UserId == userId);
            var userRolesList = userRoles.ToList(); // Convert to List
            var roleIds = userRolesList.Select(ur => ur.RoleId).ToList();

            // Batch fetch roles and explicitly convert to List<Roles>
            var roles = await _roleRepository.FindAsync(r => roleIds.Contains(r.RoleId));
            return roles.ToList(); // Explicitly convert to List<Roles>
        }

        // Helper method: Build query conditions
        private Expression<Func<TodoLists, bool>> BuildTodoCondition(
            int currentUserId,
            UserPermissions permissions,
            List<int> participatedTodoIds,
            string status)
        {
            // Create parameter expression
            var parameter = Expression.Parameter(typeof(TodoLists), "t");

            // Build condition list
            var conditions = new List<Expression>();

            // Condition 1: Owner can view their own todo items
            if (permissions.HasViewOwnPermission)
            {
                var condition1 = Expression.Equal(
                    Expression.Property(parameter, "CreatedByUserId"),
                    Expression.Constant(currentUserId)
                );
                conditions.Add(condition1);
            }

            // Condition 2: Current reviewer can view
            var condition2 = Expression.Equal(
                Expression.Property(parameter, "CurrentReviewerUserId"),
                Expression.Constant(currentUserId)
            );
            conditions.Add(condition2);

            // Condition 3: Has level 1 review permission and todo item is pending level 1 review
            if (permissions.HasReviewLevel1Permission)
            {
                var condition3 = Expression.Equal(
                    Expression.Property(parameter, "Status"),
                    Expression.Constant(ReviewAction.Pending)
                );
                conditions.Add(condition3);
            }

            // Condition 4: Has level 2 review permission and todo item is pending level 2 review
            if (permissions.HasReviewLevel2Permission)
            {
                var condition4 = Expression.Equal(
                    Expression.Property(parameter, "Status"),
                    Expression.Constant(ReviewAction.InProgress)
                );
                conditions.Add(condition4);
            }

            // Condition 5: Has participated in review
            if (participatedTodoIds.Any())
            {
                // Use Contains method
                var listContainsMethod = typeof(List<int>).GetMethod("Contains", new[] { typeof(int) });
                var condition5 = Expression.Call(
                    Expression.Constant(participatedTodoIds),
                    listContainsMethod,
                    Expression.Property(parameter, "TodoListId")
                );
                conditions.Add(condition5);
            }

            // Combine all conditions (OR relationship)
            if (conditions.Count == 0)
                return t => false;

            Expression combinedCondition = conditions[0];
            for (int i = 1; i < conditions.Count; i++)
            {
                combinedCondition = Expression.OrElse(combinedCondition, conditions[i]);
            }

            // If there's status filtering, AND it with the combined condition
            if (!string.IsNullOrEmpty(status))
            {
                var statusCondition = Expression.Equal(
                    Expression.Property(parameter, "Status"),
                    Expression.Constant(status)
                );
                combinedCondition = Expression.AndAlso(combinedCondition, statusCondition);
            }

            return Expression.Lambda<Func<TodoLists, bool>>(combinedCondition, parameter);
        }

        // Helper method: Map to ViewModel
        private List<TodoWithReviewHistoryViewModel> MapToViewModel(
            List<TodoLists> todos,
            Dictionary<int, List<Reviews>> reviewsByTodoId)
        {
            return todos.Select(todo =>
            {
                var reviewList = reviewsByTodoId.ContainsKey(todo.TodoListId)
                    ? reviewsByTodoId[todo.TodoListId]
                    : new List<Reviews>();

                return new TodoWithReviewHistoryViewModel
                {
                    Id = todo.TodoListId,
                    Title = todo.Title,
                    Status = todo.Status,
                    CreatedByUserId = todo.CreatedByUserId,
                    CurrentReviewerUserId = todo.CurrentReviewerUserId,
                    CreatedAt = todo.CreatedAt,
                    ReviewHistories = reviewList.Select(review => new ReviewHistoryViewModel
                    {
                        ReviewId = review.ReviewId,
                        ReviewerUserId = review.ReviewerUserId,
                        Action = review.Action,
                        Comment = review.Comment,
                        PreviousStatus = review.PreviousStatus,
                        NewStatus = review.NewStatus,
                        CreatedAt = review.ReviewedAt
                    }).ToList()
                };
            }).ToList();
        }

        // Permission information class
        private class UserPermissions
        {
            public bool HasViewOwnPermission { get; set; }
            public bool HasReviewLevel1Permission { get; set; }
            public bool HasReviewLevel2Permission { get; set; }
        }
    }
}