namespace TestApi.Domain.Entities
{
    public class Appointment
    {
        public Guid Id { get; set; }
        public DateTime DateOfAppointment { get; set; }
        public string Description { get; set; }
    }
}
