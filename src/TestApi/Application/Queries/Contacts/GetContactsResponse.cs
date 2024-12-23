
using ApiDocAndMock.Application.Models.Responses;

namespace TestApi.Application.Queries.Contacts
{
    public class GetContactsResponse : ApiResponseBase
    {
        public List<GetContactByIdResponse> Contacts { get; set; }
    }
}
