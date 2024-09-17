using System;
using System.Threading.Tasks;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess.Model;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts.AddContact;

public sealed class AddContactService
{
    private readonly IAddContactSession _session;
    
    public AddContactService(IAddContactSession session) => _session = session;
    
    public async Task AddContactAsync(Contact contact)
    {
        var existingContact = await _session.GetContactAsync(contact.Id);
        if (existingContact is not null)
        {
            throw new InvalidOperationException("Contact with the same ID already exists.");
        }
        
        _session.AddContact(contact);
        await _session.SaveChangesAsync();
    }
}