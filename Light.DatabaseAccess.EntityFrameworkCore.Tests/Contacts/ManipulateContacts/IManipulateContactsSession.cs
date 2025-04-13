using Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts.GetAllContacts;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess.Model;
using Light.SharedCore.DatabaseAccessAbstractions;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts.ManipulateContacts;

public interface IManipulateContactsSession : IGetAllContactsSession, ISession
{
    void RemoveContact(Contact contact);
}