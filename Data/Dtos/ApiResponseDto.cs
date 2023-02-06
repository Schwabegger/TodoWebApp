using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Dtos
{
    public sealed record ApiResponseDto
    {
        public ApiResponseDto()
        {
            ErrorMessages = new List<string>();
        }
        public bool Success { get; set; }
        public Object Content { get; set; }
        public IEnumerable<string> ErrorMessages { get; set; }
    }
}
