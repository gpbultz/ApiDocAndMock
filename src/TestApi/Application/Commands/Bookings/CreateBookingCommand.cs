using TestApi.Domain.Entities;

namespace TestApi.Application.Commands.Bookings
{
    public class CreateBookingCommand
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public Guid RoomId { get; set; }
        public Guid PrimaryContactId { get; set; }
        public int NumberOfGuests { get; set; }
    }
}
