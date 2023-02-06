using System.ComponentModel.DataAnnotations;

namespace Authentication.Dtos.Generic;

public sealed record TokenData
{
    public string JwtToken { get; set; }

    public string RefreshToken { get; set; }
}