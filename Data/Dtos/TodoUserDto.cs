using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Dtos
{
    public sealed record TodoUserDto
    {
        public string Id { get; set; } = default!;
        public string UserName { get; set; } = default!;
    }
}