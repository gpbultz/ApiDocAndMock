using ApiDocAndMock.Application.Models.Responses;

namespace ApiDocAndMock.Application.Interfaces
{
    public interface IApiResponse
    {
        PaginationMetadata? Pagination { get; set; }
        LinksContainer? Links { get; set; }
    }
}
