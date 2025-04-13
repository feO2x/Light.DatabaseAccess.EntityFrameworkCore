using System;
using System.Threading;
using System.Threading.Tasks;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess.Model;
using Light.SharedCore.DatabaseAccessAbstractions;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts.AddContact;

public interface IAddContactSession : ISession
{
    Task<Contact?> GetContactAsync(Guid id, CancellationToken cancellationToken = default);

    void AddContact(Contact contact);
}