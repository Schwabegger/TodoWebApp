using Data.Models;

namespace Core.Interfaces;

public interface ITodoUserService
{
    Task<TodoUser> GetByIdAsync(Guid id);
    Task<IEnumerable<TodoUser>> GetAllAsync();
    Task<bool> AddAsync(TodoUser todoUser);
    Task<bool> RemoveAsync(Guid id);
    Task<TodoUser> GetTodoUserByEmail(string email);
    Task<bool> UpdateTodoUserProfile(TodoUser todoUser);
    Task<TodoUser> GetByIdentityId(Guid identityId);
}