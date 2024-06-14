using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Light.EntityFrameworkCore.Tests.DatabaseAccess;
using Light.EntityFrameworkCore.Tests.DatabaseAccess.Model;

namespace Light.EntityFrameworkCore.Tests.Contacts.GetAllContacts;

public sealed class EfGetAllContactsReadUncommittedSession : EfAsyncReadOnlySession<MyDbContext>.WithTransaction,
                                                             IGetAllContactsSession
{
    public EfGetAllContactsReadUncommittedSession(MyDbContext dbContext) : base(
        dbContext,
        IsolationLevel.ReadUncommitted
    ) { }

    public async Task<List<Contact>> GetContactsAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync(cancellationToken);
        return await dbContext.GetContactsAsync(cancellationToken);
    }
}