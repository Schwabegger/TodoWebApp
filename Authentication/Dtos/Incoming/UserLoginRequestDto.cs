using System.ComponentModel.DataAnnotations;

namespace Authentication.Dtos.Incoming;

public sealed record UserLoginRequestDto
{
    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}