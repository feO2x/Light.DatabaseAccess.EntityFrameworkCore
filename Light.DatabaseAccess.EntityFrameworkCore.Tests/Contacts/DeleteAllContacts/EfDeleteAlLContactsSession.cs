﻿using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess;
using Light.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts.DeleteAllContacts;

public sealed class EfDeleteAlLContactsSession : EfAsyncSession<MyDbContext>.WithTransaction, IDeleteAllContactsSession
{
    public EfDeleteAlLContactsSession(MyDbContext dbContext) : base(dbContext, IsolationLevel.Serializable) { }
    
    public async Task DeleteAllContactsAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync(cancellationToken);
        await dbContext.Contacts.ExecuteDeleteAsync(cancellationToken);
    }
}