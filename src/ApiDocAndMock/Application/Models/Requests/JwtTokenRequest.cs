using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Application.Models.Requests
{
    public class JwtTokenRequest
    {
        public List<string> Roles { get; set; } = new List<string>();
    }
}
