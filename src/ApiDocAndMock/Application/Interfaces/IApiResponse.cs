using ApiDocAndMock.Application.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Application.Interfaces
{
    public interface IApiResponse
    {
        PaginationMetadata? Pagination { get; set; }
        LinksContainer? Links { get; set; }
    }
}
