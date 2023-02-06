using Core.Interfaces;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core.Service;

[Obsolete("IGenericService objects must be updated")]
public class RefreshTokenService : IRefreshTokenService
{
    private TodoDbContext _context;
    
    public RefreshTokenService(TodoDbContext context)
    {
        _context = context;
    }
    
    public async Task<bool> AddAsync(RefreshToken entity)
    {
        await _context.RefreshTokens.AddAsync(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<RefreshToken>> GetAllAsync()
    {
        try
        {
            return await _context.RefreshTokens.Where(x => x.Status == 1).AsNoTracking().ToListAsync();
        }
        catch (Exception ex)
        {
            return new List<RefreshToken>();
        }
    }

    public async Task<RefreshToken> GetByRefreshTokenAsync(string refreshToken)
    {
        try
        {
            return await _context.RefreshTokens.Where(x => x.Token.ToLower() == refreshToken.ToLower()).AsNoTracking().FirstOrDefaultAsync()!;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public async Task<bool> MarkRefresTokenAsUsed(RefreshToken refreshToken)
    {
        try
        {
            /// WOS ????!?!?
            var token = await _context.RefreshTokens.Where(x => x.Token.ToLower() == refreshToken.Token.ToLower()).AsNoTracking().FirstOrDefaultAsync()!;
            if (token == null)
                return false;
            token.IsUsed = refreshToken.IsUsed;
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public Task<RefreshToken> GetAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RemoveAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Upsert(RefreshToken entity)
    {
        throw new NotImplementedException();
    }
}