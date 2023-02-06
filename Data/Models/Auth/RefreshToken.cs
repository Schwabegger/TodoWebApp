using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models;

public sealed record RefreshToken : BaseModel
{
    public string UserId { get; set; } // UserId when logged in
    public string Token { get; set; }
    public string JwtId { get; set; } // the id generated  when a jwt id has been requested
    public bool IsUsed { get; set; } // To make sure that the token is only used once
    public bool IsRevoked { get; set; } // make sure thy are valid
    public DateTime ExpireyDate { get; set; }

    [ForeignKey(nameof(UserId))]
    public IdentityUser User { get; set; }
}