namespace TestApi.Application.Queries.Hotels
{
    public class GetHotelsResponse
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<GetHotelByIdResponse> Hotels { get; set; } = new List<GetHotelByIdResponse>();
    }
}
