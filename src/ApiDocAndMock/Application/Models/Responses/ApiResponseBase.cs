using ApiDocAndMock.Application.Interfaces;
using System.Text.Json.Serialization;

namespace ApiDocAndMock.Application.Models.Responses
{
    public abstract class ApiResponseBase : IApiResponse
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PaginationMetadata? Pagination { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public LinksContainer? Links { get; set; }
    }
}
