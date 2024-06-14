using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess.Model;
using Light.EntityFrameworkCore;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts.ManipulateContacts;

public sealed class EfManipulateContactsSession : EfAsyncSession<MyDbContext>, IManipulateContactsSession
{
    public EfManipulateContactsSession(MyDbContext dbContext) : base(dbContext) { }
    public Task<List<Contact>> GetContactsAsync(CancellationToken cancellationToken = default) =>
        DbContext.GetContactsAsync(cancellationToken);

    public void RemoveContact(Contact contact) => DbContext.Contacts.Remove(contact);
}