
using TestApi.Domain.Entities;

namespace TestApi.Application.Queries.Contacts
{
    public class GetContactByIdResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Region { get; set; }

        public string PostalCode { get; set; }
        public Dictionary<Guid, Appointment> Appointments { get; set; } = new Dictionary<Guid, Appointment>();
    }
}
