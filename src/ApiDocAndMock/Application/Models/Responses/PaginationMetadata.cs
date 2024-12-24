using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Application.Models.Responses
{
    public class PaginationMetadata
    {
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public string? First { get; set; }
        public string? Last { get; set; }
        public string? Next { get; set; }
        public string? Prev { get; set; }
    }
}
