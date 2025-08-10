using System;

namespace DistriCatalogoAPI.Domain.Exceptions
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(int userId) 
            : base($"No se encontró el usuario con ID {userId}")
        {
            UserId = userId;
        }

        public UserNotFoundException(string email) 
            : base($"No se encontró el usuario con email {email}")
        {
            Email = email;
        }

        public int? UserId { get; }
        public string Email { get; }
    }
}