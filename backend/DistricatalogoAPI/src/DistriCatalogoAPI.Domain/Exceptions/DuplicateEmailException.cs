using System;

namespace DistriCatalogoAPI.Domain.Exceptions
{
    public class DuplicateEmailException : Exception
    {
        public DuplicateEmailException(string email) 
            : base($"Ya existe un usuario activo con el email {email}")
        {
            Email = email;
        }

        public string Email { get; }
    }
}