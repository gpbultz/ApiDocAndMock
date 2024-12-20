using System.ComponentModel.DataAnnotations;

namespace TestApi.Application.Queries.Hotels
{
    public class GetHotelsQuery
    {
        public int? Page { get; set; }

        public int? PageSize { get; set; }

        [Required]
        public string? City { get; set; }

        public string? Region { get; set; }

        public string? PostalCode { get; set; }

    }
}
