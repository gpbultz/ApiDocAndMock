using ApiDocAndMock.Application.Models.Responses;

namespace ApiDocAndMock.Infrastructure.Extensions
{
    public static class ApiResponseExtensions
    {
        public static T WithPaginationAndLinks<T>(
        this T response,
        int totalCount,
        int pageSize,
        int currentPage,
        string? resourcePath = null,
        bool includeLinks = true,
        bool includePages = true)
        where T : class
        {
            if (response is ApiResponseBase apiResponse)
            {
                var path = resourcePath ?? "/";

                // Add Pagination
                if (includePages)
                {
                    int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                    apiResponse.Pagination = new PaginationMetadata
                    {
                        TotalCount = totalCount,
                        PageSize = pageSize,
                        PageNumber = currentPage,
                        TotalPages = totalPages,
                        First = $"{path}?page=1",
                        Last = $"{path}?page={totalPages}",
                        Next = currentPage < totalPages ? $"{path}?page={currentPage + 1}" : null,
                        Prev = currentPage > 1 ? $"{path}?page={currentPage - 1}" : null
                    };
                }

                // Add HATEOAS Links
                if (includeLinks)
                {
                    apiResponse.Links = new LinksContainer
                    {
                        Self = path,
                        Update = $"{path}/update",
                        Delete = $"{path}/delete"
                    };
                }
            }

            return response;
        }
    }
}
