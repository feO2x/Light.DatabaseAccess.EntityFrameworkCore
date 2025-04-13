using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess.Model;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts.GetAllContacts;

public interface IGetAllContactsSession : IAsyncDisposable
{
    Task<List<Contact>> GetContactsAsync(CancellationToken cancellationToken = default);
}