using System.ComponentModel.DataAnnotations;

namespace Authentication.Dtos.Incoming;

public sealed record TokenRequestDto
{
    [Required]
    public string Token { get; set; }

    [Required]
    public string RefreshToken { get; set; }
}