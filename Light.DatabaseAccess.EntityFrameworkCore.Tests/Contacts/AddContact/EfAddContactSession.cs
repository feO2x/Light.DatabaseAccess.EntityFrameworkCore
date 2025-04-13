using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess.Model;
using Microsoft.EntityFrameworkCore;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts.AddContact;

public sealed class EfAddContactSession : EfSession<MyDbContext>.WithTransaction, IAddContactSession
{
    public EfAddContactSession(MyDbContext dbContext) : base(dbContext, IsolationLevel.RepeatableRead) { }
    
    public async Task<Contact?> GetContactAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync(cancellationToken);
        return await dbContext.Contacts.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public void AddContact(Contact contact) => DbContext.Contacts.Add(contact);
}