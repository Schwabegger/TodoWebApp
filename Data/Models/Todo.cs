using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models;

public sealed record Todo
{
    public int Id { get; set; }
    [Required]
    public string Title { get; set; } = default!;
    [Required]
    public List<TodoTask> Tasks { get; set; } = new();

    [Required]
    public string OwnerId { get; set; } = default!;
}