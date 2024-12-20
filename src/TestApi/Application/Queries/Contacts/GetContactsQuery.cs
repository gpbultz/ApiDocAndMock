﻿using TestApi.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace TestApi.Application.Queries.Contacts
{
    public class GetContactsQuery
    {
        public int? Page { get; set; }

        public int? PageSize { get; set; }

        [Required]
        public string? City { get; set; }

        public string? Region { get; set; }

        public string? PostalCode { get; set; }

    }
}