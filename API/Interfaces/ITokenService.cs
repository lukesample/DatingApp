using DatingApp.API.Entities;

namespace DatingApp.Interfaces
{
    //an interface is kind of like a contract between itself and any class that implements it
    //this contract states that any class that implements it will implement the properties, methods, or events
    //does not contain logic, only the signatures
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}