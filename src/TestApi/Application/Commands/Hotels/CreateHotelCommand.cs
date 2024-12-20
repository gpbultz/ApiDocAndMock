using TestApi.Domain.Entities;

namespace TestApi.Application.Commands.Hotels
{
    public class CreateHotelCommand
    {
        public List<Room> Rooms { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public List<Booking> Bookings { get; set; }
    }
}
