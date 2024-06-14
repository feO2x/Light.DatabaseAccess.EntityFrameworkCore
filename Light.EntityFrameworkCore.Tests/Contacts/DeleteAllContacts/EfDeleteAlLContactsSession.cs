using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Light.EntityFrameworkCore.Tests.DatabaseAccess;
using Microsoft.EntityFrameworkCore;

namespace Light.EntityFrameworkCore.Tests.Contacts.DeleteAllContacts;

public sealed class EfDeleteAlLContactsSession : EfAsyncSession<MyDbContext>.WithTransaction, IDeleteAllContactsSession
{
    public EfDeleteAlLContactsSession(MyDbContext dbContext) : base(dbContext, IsolationLevel.Serializable) { }
    
    public async Task DeleteAllContactsAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync(cancellationToken);
        await dbContext.Contacts.ExecuteDeleteAsync(cancellationToken);
    }
}