using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Light.EntityFrameworkCore.Tests.DatabaseAccess.Model;

namespace Light.EntityFrameworkCore.Tests.Contacts.GetAllContacts;

public sealed class GetAllContactsService
{
    private readonly IGetAllContactsSession _session;

    public GetAllContactsService(IGetAllContactsSession session)
    {
        _session = session;
    }
    
    public Task<List<Contact>> GetAllContactsAsync(CancellationToken cancellationToken = default)
    {
        return _session.GetContactsAsync(cancellationToken);
    }
}