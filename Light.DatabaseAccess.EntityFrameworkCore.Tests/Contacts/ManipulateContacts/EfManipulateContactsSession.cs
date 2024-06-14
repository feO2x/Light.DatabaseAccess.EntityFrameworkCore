using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Light.EntityFrameworkCore.Tests.DatabaseAccess;
using Light.EntityFrameworkCore.Tests.DatabaseAccess.Model;

namespace Light.EntityFrameworkCore.Tests.Contacts.ManipulateContacts;

public sealed class EfManipulateContactsSession : EfAsyncSession<MyDbContext>, IManipulateContactsSession
{
    public EfManipulateContactsSession(MyDbContext dbContext) : base(dbContext) { }
    public Task<List<Contact>> GetContactsAsync(CancellationToken cancellationToken = default) =>
        DbContext.GetContactsAsync(cancellationToken);

    public void RemoveContact(Contact contact) => DbContext.Contacts.Remove(contact);
}