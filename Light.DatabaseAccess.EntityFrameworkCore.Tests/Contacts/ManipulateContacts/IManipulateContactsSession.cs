using Light.EntityFrameworkCore.Tests.Contacts.GetAllContacts;
using Light.EntityFrameworkCore.Tests.DatabaseAccess.Model;
using Light.SharedCore.DatabaseAccessAbstractions;

namespace Light.EntityFrameworkCore.Tests.Contacts.ManipulateContacts;

public interface IManipulateContactsSession : IGetAllContactsSession, IAsyncSession
{
    void RemoveContact(Contact contact);
}