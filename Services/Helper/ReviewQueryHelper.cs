using Common.DTOs.ReviewTodoLists.Responses;
using Common.Enums;
using Common.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.MyRepository;
using Services.Interfaces;
using System.Linq.Expressions;

namespace Services.Helpers
{
    public class ReviewQueryHelper
    {
        private readonly IRepository<TodoLists> _todoRepository;
        private readonly IRepository<Reviews> _reviewRepository;
        private readonly IRepository<Users_Roles> _userRoleRepository;
        private readonly IRepository<Roles> _roleRepository;
        private readonly IRbacService _rbacService;

        public ReviewQueryHelper(
            IRepository<TodoLists> todoRepository,
            IRepository<Reviews> reviewRepository,
            IRepository<Users_Roles> userRoleRepository,
            IRepository<Roles> roleRepository,
            IRbacService rbacService)
        {
            _todoRepository = todoRepository;
            _reviewRepository = reviewRepository;
            _userRoleRepository = userRoleRepository;
            _rbacService = rbacService;
            _roleRepository = roleRepository;
        }

        // Core method: Get review todo items in one go
        public async Task<List<TodoWithReviewHistoryViewModel>> GetReviewTodosAsync(
            int currentUserId,
            string status = null)
        {
            try
            {
                // 1. Get user permissions
                var permissions = await GetUserPermissionsAsync(currentUserId);

                // 2. Get all todo items
                var allTodos = await _todoRepository.GetAllAsync();

                // 3. Filter eligible todo items
                var eligibleTodos = new List<TodoLists>();

                foreach (var todo in allTodos)
                {
                    if (!string.IsNullOrEmpty(status) && todo.Status != status)
                        continue;

                    if (await IsTodoEligibleForUserAsync(todo, currentUserId, permissions))
                    {
                        eligibleTodos.Add(todo);
                    }
                }

                if (!eligibleTodos.Any())
                    return new List<TodoWithReviewHistoryViewModel>();

                // 4. Batch retrieve review records
                var todoIds = eligibleTodos.Select(t => t.TodoListId).ToList();
                var allReviews = await _reviewRepository.FindAsync(r => todoIds.Contains(r.TodoId));
                var reviewsByTodoId = allReviews
                    .GroupBy(r => r.TodoId)
                    .ToDictionary(g => g.Key, g => g.OrderBy(r => r.ReviewedAt).ToList());

                // 5. Assemble response
                return MapToViewModel(eligibleTodos, reviewsByTodoId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetReviewTodosAsync: {ex.Message}");
                throw;
            }
        }

        // Check if a todo item is visible to the user
        private async Task<bool> IsTodoEligibleForUserAsync(TodoLists todo, int currentUserId, UserPermissions permissions)
        {
            // 1. Creator can see all items they created
            if (todo.CreatedByUserId == currentUserId)
                return true;

            // 2. Current reviewer can see
            if (todo.CurrentReviewerUserId == currentUserId)
                return true;

            // 3. If previously participated in review, can see
            var participatedReviews = await _reviewRepository.FindAsync(r =>
                r.TodoId == todo.TodoListId && r.ReviewerUserId == currentUserId);
            if (participatedReviews.Any())
                return true;

            // 4. Check permissions based on status
            if ((todo.Status == ReviewAction.Pending || todo.Status == ReviewAction.ReturnedToLevel1)
                && permissions.HasReviewLevel1Permission)
                return true;

            if (todo.Status == ReviewAction.InProgress && permissions.HasReviewLevel2Permission)
                return true;

            if (todo.Status == ReviewAction.Returned && permissions.HasReviewLevel1Permission)
                return true;

            if (todo.Status == ReviewAction.Approved && permissions.HasReviewLevel2Permission)
                return true;

            return false;
        }

        // Get user permissions
        private async Task<UserPermissions> GetUserPermissionsAsync(int userId)
        {
            var userRoles = await GetUserRolesWithRoleAsync(userId);
            var permissions = new UserPermissions();

            if (userRoles.Any())
            {
                // Check level 1 review permission
                foreach (var userRole in userRoles)
                {
                    if (userRole.Role == null) continue;

                    var result = await _rbacService.HasPermissionAsync(
                        userRole.Role.RoleId,
                        Permission.TodoReviewLevel1.GetPermissionName());

                    if (result.IsSuccess && result.Data)
                    {
                        permissions.HasReviewLevel1Permission = true;
                        break;
                    }
                }

                // Check level 2 review permission
                foreach (var userRole in userRoles)
                {
                    if (userRole.Role == null) continue;

                    var result = await _rbacService.HasPermissionAsync(
                        userRole.Role.RoleId,
                        Permission.TodoReviewLevel2.GetPermissionName());

                    if (result.IsSuccess && result.Data)
                    {
                        permissions.HasReviewLevel2Permission = true;
                        break;
                    }
                }
            }

            return permissions;
        }

        // Get user roles with navigation property
        private async Task<List<Users_Roles>> GetUserRolesWithRoleAsync(int userId)
        {
            try
            {
                var userRoles = await _userRoleRepository.FindWithIncludesAsync(
                    ur => ur.UserId == userId,
                    ur => ur.Role);

                if (userRoles == null)
                {
                    Console.WriteLine("userRoles is null");
                    return new List<Users_Roles>();
                }

                var userRolesList = userRoles.ToList();
                Console.WriteLine($"Found {userRolesList.Count} user roles for user {userId}");

                return userRolesList
                    .Where(ur => ur != null && ur.Role != null)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user roles in ReviewQueryHelper: {ex.Message}");
                return new List<Users_Roles>();
            }
        }

        // Map to ViewModel
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
            public bool HasReviewLevel1Permission { get; set; }
            public bool HasReviewLevel2Permission { get; set; }
        }
    }
}