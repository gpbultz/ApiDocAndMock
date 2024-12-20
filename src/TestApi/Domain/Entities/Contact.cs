﻿namespace TestApi.Domain.Entities
{
    public class Contact
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }

        public string Region { get; set; }
        public string PostalCode { get; set; }

    }
}
