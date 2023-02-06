using Data.Dtos;
using Data.Models;

namespace Core.Interfaces
{
    public interface ITodoTaskService
    {
        Task<IEnumerable<TodoTaskDto>> GetAllAsync();
        Task<IEnumerable<TodoTaskDto>> GetAllTasksByTodoIdAsync(int todoId);
        Task<TodoTaskDto> GetByIdAsync(int id);
        Task<TodoTask> CreateTodoTaskAsync(TodoTaskDto todoTaskDto, int todoId);
        Task<bool> UpdateTodoTaskAsync(TodoTaskDto todoTaskDto);
        Task<bool> RemoveByIdTaskAsync(int id);
    }
}