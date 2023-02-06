using Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Dtos;
    
    
public sealed record TodoDto
{
    public int Id { get; set; }
    [Required]
    public string Title { get; set; } = default!;
    [Required]
    public IEnumerable<TodoTaskDto> Tasks { get; set; }
}