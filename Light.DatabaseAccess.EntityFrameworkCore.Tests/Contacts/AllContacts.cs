using System;
using System.Collections.Generic;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess.Model;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts;

public static class AllContacts
{
    public static List<Contact> DefaultContacts { get; } =
    [
        new ()
        {
            Id = new Guid("E5DE02EA-8F00-4675-B21F-6D3CEBD100EF"),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@gmail.com"
        },
        new ()
        {
            Id = new Guid("5C9714D2-2356-4448-A14B-125F1A70C87D"),
            FirstName = "Jane",
            LastName = "Smith"
        },
        new ()
        {
            Id = new Guid("FCCEA57A-2D70-4FFB-B101-1EA353A9ACAA"),
            FirstName = "Alice"
        }
    ];
    
    public static Contact NewContact { get; } =
        new ()
        {
            Id = new Guid("733CAFF4-DD15-49F8-8FD5-2C543A5238D8"),
            FirstName = "Adam",
            LastName = "Blue"
        };
}