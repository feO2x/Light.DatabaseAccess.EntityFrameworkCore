using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Light.EntityFrameworkCore.Tests.DatabaseAccess;
using Light.EntityFrameworkCore.Tests.DatabaseAccess.Model;

namespace Light.EntityFrameworkCore.Tests.Contacts.GetAllContacts;

public sealed class EfGetAllContactsSession : EfAsyncReadOnlySession<MyDbContext>, IGetAllContactsSession
{
    public EfGetAllContactsSession(MyDbContext dbContext) : base(dbContext) { }

    // Please note: to keep this test simple, this method loads all contacts into memory.
    // In a production scenario, you shouldn't do that but instead use paging to limit the number of entities loaded.
    public Task<List<Contact>> GetContactsAsync(CancellationToken cancellationToken = default) =>
        DbContext.GetContactsAsync(cancellationToken);
}