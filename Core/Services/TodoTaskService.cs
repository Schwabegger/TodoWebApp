using Core.Interfaces;
using Core.Mapping;
using Data.Dtos;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Services
{
    public class TodoTaskService : ITodoTaskService
    {
        private TodoDbContext _context;
        public TodoTaskService(TodoDbContext context)
        {
            _context = context;
        }

        public async Task<TodoTask> CreateTodoTaskAsync(TodoTaskDto todoTaskDto, int todoId)
        {
            var todoTask = todoTaskDto.AsEntity();
            todoTask.TodoId = todoId;
            await _context.TodoTasks.AddAsync(todoTask);
            await _context.SaveChangesAsync();
            return todoTask;
        }

        public async Task<IEnumerable<TodoTaskDto>> GetAllAsync()
        {
            return _context.TodoTasks.AsNoTracking().Select(t => t.AsDto()).ToList();
        }

        public async Task<IEnumerable<TodoTaskDto>> GetAllTasksByTodoIdAsync(int todoId)
        {
            return await _context.TodoTasks.AsNoTracking().Where(t => t.TodoId == todoId).Select(t => t.AsDto()).ToListAsync();
        }

        public async Task<TodoTaskDto> GetByIdAsync(int id)
        {
            return await _context.TodoTasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id).ContinueWith(t => t.Result.AsDto());
            //return await _context.TodoTasks.AsNoTracking().Where(t => t.Id == id).Select(t => t.AsDto()).FirstOrDefaultAsync();
        }

        public async Task<bool> RemoveByIdTaskAsync(int id)
        {
            _context.TodoTasks.Remove(await _context.TodoTasks.FirstOrDefaultAsync(t => t.Id == id));
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateTodoTaskAsync(TodoTaskDto todoTaskDto)
        {
            var todoTask = _context.TodoTasks.FirstOrDefault(t => t.Id == todoTaskDto.Id);
            todoTask.TaskName = todoTaskDto.TaskName;
            todoTask.IsCompleted = todoTaskDto.IsCompleted;
            _context.Update(todoTask);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
