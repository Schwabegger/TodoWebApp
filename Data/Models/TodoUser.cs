using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Data.Models;

public sealed record TodoUser
{
    public int Id { get; set; }
    [Required]
    public Guid IdentityId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    [Required]
    public string Email { get; set; }
    public int Status { get; set; } = 1;
}