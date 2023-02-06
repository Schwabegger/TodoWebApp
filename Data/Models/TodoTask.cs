using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public sealed record TodoTask
    {
        public int Id { get; set; }
        [Required]
        public int TodoId { get; set; }
        [Required]
        public string TaskName { get; set; }

        public bool IsCompleted { get; set; } = false;
    }
}
