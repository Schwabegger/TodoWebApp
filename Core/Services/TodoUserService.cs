using Core.Interfaces;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Core.Service;

[Obsolete("IGenericService objects must be updated")]
public class TodoUserService : ITodoUserService
{
    private TodoDbContext _context;
    public TodoUserService(TodoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TodoUser>> GetAllAsync()
    {
        try
        {
            return await _context.TodoUsers.Where(x => x.Status == 1).AsNoTracking().ToListAsync();
        }
        catch (Exception ex)
        {
            return new List<TodoUser>();
        }
    }
    
    public async Task<TodoUser> GetByIdAsync(Guid id)
    {
        return _context.TodoUsers.Where(x => x.IdentityId == id).FirstOrDefault();
    }

    public async Task<TodoUser> GetByIdentityId(Guid identityId)
    {
        try
        {
            return await _context.TodoUsers.Where(x => x.Status == 1 && x.IdentityId == identityId).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public async Task<TodoUser> GetTodoUserByEmail(string email)
    {
        return await _context.TodoUsers.FindAsync(email);
    }
    
    public async Task<bool> UpdateTodoUserProfile(TodoUser todoUser)
    {
        try
        {
            var existingTodoUser = await _context.TodoUsers.Where(x => x.Status == 1 && x.IdentityId == todoUser.IdentityId).FirstOrDefaultAsync();
            if (existingTodoUser == null)
                return false;

            existingTodoUser.FirstName = todoUser.FirstName;
            existingTodoUser.LastName = todoUser.LastName;

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<bool> AddAsync(TodoUser todoUser)
    {
        await _context.TodoUsers.AddAsync(todoUser);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> RemoveAsync(Guid id)
    {
        _context.TodoUsers.Where(x => x.IdentityId == id).FirstOrDefault().Status = 0;
        _context.SaveChanges();
        return true;
    }
}