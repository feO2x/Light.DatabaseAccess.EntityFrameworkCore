using System;
using System.Collections.Generic;
using Light.SharedCore.Entities;

namespace Light.EntityFrameworkCore.Tests.DatabaseAccess.Model;

public sealed class Contact : GuidEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Email { get; set; }

    public static List<Contact> DefaultContacts { get; } =
    [
        new Contact
        {
            Id = new Guid("E5DE02EA-8F00-4675-B21F-6D3CEBD100EF"), 
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@gmail.com"
        },
        new Contact
        {
            Id = new Guid("5C9714D2-2356-4448-A14B-125F1A70C87D"),
            FirstName = "Jane",
            LastName = "Smith"
        },
        new Contact
        {
            Id = new Guid("FCCEA57A-2D70-4FFB-B101-1EA353A9ACAA"),
            FirstName = "Alice",
        }
    ];
}