using System.Threading;
using System.Threading.Tasks;

namespace Light.EntityFrameworkCore.Tests.Contacts.DeleteAllContacts;

public sealed class DeleteAllContactsService
{
    private readonly IDeleteAllContactsSession _session;

    public DeleteAllContactsService(IDeleteAllContactsSession session)
    {
        _session = session;
    }
    
    public async Task DeleteAllContactsAsync(CancellationToken cancellationToken = default)
    {
        await _session.DeleteAllContactsAsync(cancellationToken);
        await _session.SaveChangesAsync(cancellationToken);
    }
}