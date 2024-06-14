using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess.Model;
using Microsoft.EntityFrameworkCore;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess;

public static class CommonQueries
{
    public static Task<List<Contact>> GetContactsAsync(this MyDbContext dbContext, CancellationToken cancellationToken = default) =>
        dbContext.Contacts.ToListAsync(cancellationToken);
}