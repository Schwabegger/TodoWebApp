using Data.Models;

namespace Core.Interfaces;

public interface IRefreshTokenService : IGenericService<RefreshToken>
{
    Task<RefreshToken> GetByRefreshTokenAsync(string refreshToken);
    Task<bool> MarkRefresTokenAsUsed(RefreshToken refreshToken);
}