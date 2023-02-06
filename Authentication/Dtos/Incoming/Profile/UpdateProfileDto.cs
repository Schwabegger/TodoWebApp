using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.Dtos.Incoming.Profile
{
    public sealed record UpdateProfileDto
    {
        public string Country { get; set; }
        public string Address { get; set; }
        public string MobileNumber { get; set; }
        public string Gender { get; set; }
    }
}