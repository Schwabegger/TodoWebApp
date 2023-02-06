using Data.Dtos;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ITodoService
    {
        public Task<IEnumerable<TodoDto>> GetAllTodosAsync();
        public Task<TodoDto> GetTodoAsync(int id, string userId);
        public Task CreateTodoAsync(TodoDto todoDto, string userId);
        public Task UpdateTodoAsync(TodoDto todoDto, string userId);
        public Task RemoveTodoAsync(int id, string userId);
        public Task<IEnumerable<TodoDto>> GetTodosByUserIdAsync(string userId);
    }
}