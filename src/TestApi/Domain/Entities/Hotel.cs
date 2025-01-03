namespace TestApi.Domain.Entities
{
    public class Hotel
    {
        public Guid Id { get; set; }
        public List<Room> Rooms { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public List<Booking> Bookings { get; set; }
        public Tuple<double, double> Coordinates { get; set; }
        public Dictionary<Guid, string> Metadata { get; set; }
    }
}
