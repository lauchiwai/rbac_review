using Common.DTOs;
using Common.DTOs.Template.Requests;
using Common.DTOs.Template.Response;
using Common.DTOs.Todo.Requests;
using Common.DTOs.Todo.Response;
using Common.DTOs.Transition.Requests;
using Common.DTOs.Transition.Response;

namespace Services.Interfaces;

public interface ITodoService
{
    Task<ResultDto<TodoCreateResponse>> CreateTodoAsync(TodoCreateRequest request);

    Task<ResultDto<bool>> HasPermissionAsync(int userId, string permissionName);
}