using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess;
using Microsoft.EntityFrameworkCore;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts.DeleteAllContacts;

public sealed class EfDeleteAllContactsSession : EfAsyncSession<MyDbContext>.WithTransaction, IDeleteAllContactsSession
{
    public EfDeleteAllContactsSession(MyDbContext dbContext) : base(dbContext, IsolationLevel.Serializable) { }
    
    public async Task DeleteAllContactsAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync(cancellationToken);
        await dbContext.Contacts.ExecuteDeleteAsync(cancellationToken);
    }
}