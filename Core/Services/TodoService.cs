using Core.Interfaces;
using Core.Mapping;
using Data.Dtos;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class TodoService : ITodoService
    {
        private readonly TodoDbContext _context;
        private readonly ITodoTaskService _todoTaskService;

        public TodoService(TodoDbContext context, ITodoTaskService todoTaskService)
        {
            _context = context;
            _todoTaskService = todoTaskService;
        }

        public async Task CreateTodoAsync(TodoDto todoDto, string userId)
        {
            var todo = todoDto.AsEntity(userId);
            
            await _context.Todos.AddAsync(todo);
            await _context.SaveChangesAsync();
            
            foreach (var todooTaskDto in todoDto.Tasks)
                todo.Tasks.Add(await _todoTaskService.CreateTodoTaskAsync(todooTaskDto, todo.Id));
        }

        public async Task<TodoDto> GetTodoAsync(int id, string userId)
        {
            var todo = await _context.Todos.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == userId);
            return todo.AsDto();
        }

        public async Task<IEnumerable<TodoDto>> GetAllTodosAsync()
        {
            var todos = _context.Todos.AsNoTracking().ToList().ConvertAll(new Converter<Todo, TodoDto>(t => t.AsDto()));
            //var todos = _context.Todos.AsNoTracking().Select(t => t.AsDto()).ToListAsync();
            return todos;
        }

        public async Task UpdateTodoAsync(TodoDto todoDto, string userId)
        {
            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == todoDto.Id && t.OwnerId == userId);

            todo.Title = todoDto.Title;
            //todo.Tasks = todoDto.Tasks.ConvertAll(new Converter<TodoTaskDto, TodoTask>(t => t.AsEntity()));

            _context.Todos.Update(todo);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveTodoAsync(int id, string userId)
        {
            _context.Todos.Remove(await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == userId));
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TodoDto>> GetTodosByUserIdAsync(string userId)
        {
            var todos = _context.Todos.AsNoTracking().Where(t => t.OwnerId == userId.ToString()).ToList().ConvertAll(new Converter<Todo, TodoDto>(t => t.AsDto()));
            foreach(var todo in todos)
                todo.Tasks = await _todoTaskService.GetAllTasksByTodoIdAsync(todo.Id);
            return todos;
        }
    }
}
