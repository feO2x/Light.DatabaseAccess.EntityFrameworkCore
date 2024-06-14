using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess.Model;
using Light.SharedCore.DatabaseAccessAbstractions;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts.GetAllContacts;

public interface IGetAllContactsSession : IAsyncReadOnlySession
{
    Task<List<Contact>> GetContactsAsync(CancellationToken cancellationToken = default);
}