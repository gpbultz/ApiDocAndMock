namespace TestApi.Domain.Entities
{
    public class Booking
    {
        public Guid Id { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public Room Room { get; set; }
        public Contact PrimaryContact { get; set; }
        public int NumberOfGuests { get; set; }

    }
}
