using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Application.Models.Responses
{
    public class LinksContainer
    {
        public string? Self { get; set; }
        public string? Update { get; set; }
        public string? Delete { get; set; }
    }
}
