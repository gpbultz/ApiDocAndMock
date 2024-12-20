namespace TestApi.Application.Queries.Contacts
{
    public class GetContactsResponse
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<GetContactByIdResponse> Contacts { get; set; }
    }
}
