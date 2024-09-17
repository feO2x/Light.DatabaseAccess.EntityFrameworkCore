using Light.SharedCore.Entities;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess.Model;

public sealed class Contact : GuidEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Email { get; set; }
}