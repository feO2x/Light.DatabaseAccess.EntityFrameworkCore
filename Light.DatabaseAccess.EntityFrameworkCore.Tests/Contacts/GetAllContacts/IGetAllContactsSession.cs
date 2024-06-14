using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Light.EntityFrameworkCore.Tests.DatabaseAccess.Model;
using Light.SharedCore.DatabaseAccessAbstractions;

namespace Light.EntityFrameworkCore.Tests.Contacts.GetAllContacts;

public interface IGetAllContactsSession : IAsyncReadOnlySession
{
    Task<List<Contact>> GetContactsAsync(CancellationToken cancellationToken = default);
}