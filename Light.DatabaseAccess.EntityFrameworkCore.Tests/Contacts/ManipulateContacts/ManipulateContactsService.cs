using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess.Model;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts.ManipulateContacts;

public sealed class ManipulateContactsService
{
    private readonly IManipulateContactsSession _session;

    public ManipulateContactsService(IManipulateContactsSession session)
    {
        _session = session;
    }

    public async Task ManipulateContactsAsync(CancellationToken cancellationToken = default)
    {
        var contacts = await _session.GetContactsAsync(cancellationToken);

        contacts[1].LastName = "Porter";
        
        _session.RemoveContact(contacts.Last());
        await _session.SaveChangesAsync(cancellationToken);
    }

    public static List<Contact> GetExpectedManipulatedContacts()
    {
        var contactsCopy = Contact.DefaultContacts.ToList();
        contactsCopy[1].LastName = "Porter";
        contactsCopy.RemoveAt(2);
        return contactsCopy;
    }
}