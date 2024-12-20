
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace TestApi.Application.Commands.Contacts
{
    public class CreateContactCommand
    {
        [Required]
        [SwaggerSchema("The full name of the contact.")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [SwaggerSchema("The contact's email address.")]
        public string Email { get; set; }

        [Required]
        [Phone]
        [SwaggerSchema("The contact's phone number.")]
        public string Phone { get; set; }

        [Required]
        [SwaggerSchema("The street address of the contact.")]
        public string Address { get; set; }

        [Required]
        [SwaggerSchema("The city where the contact resides.")]
        public string City { get; set; }

        [SwaggerSchema("The region or state of the contact.")]
        public string Region { get; set; }

        [Required]
        [SwaggerSchema("The postal or ZIP code of the contact.")]
        public string PostalCode { get; set; }
    }
}
