using Common.DTOs.TodoLists.Requests;
using Common.DTOs.TodoLists.Responses;
using Common.DTOs;

namespace Services.Interfaces;

public interface ITodoListsService
{
    Task<ResultDto<TodoViewModel>> CreateTodoAsync(CreateTodoRequest request);

    Task<ResultDto<TodoViewModel>> GetTodoAsync(int todoId, int currentUserRoleId);

    Task<ResultDto<List<TodoViewModel>>> GetAllTodosAsync(GetTodosRequest request);

    Task<ResultDto<TodoViewModel>> UpdateTodoAsync(UpdateTodoRequest request);

    Task<ResultDto> DeleteTodoAsync(int todoId, int currentUserRoleId);
}