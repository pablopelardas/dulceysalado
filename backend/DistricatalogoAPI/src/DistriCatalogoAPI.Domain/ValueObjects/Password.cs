using System;
using BCrypt.Net;

namespace DistriCatalogoAPI.Domain.ValueObjects
{
    public class Password
    {
        private const int MinLength = 8;
        
        public static string HashPassword(string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
                throw new ArgumentException("La contraseña no puede estar vacía", nameof(plainPassword));

            if (plainPassword.Length < MinLength)
                throw new ArgumentException($"La contraseña debe tener al menos {MinLength} caracteres", nameof(plainPassword));

            return BCrypt.Net.BCrypt.HashPassword(plainPassword);
        }

        public static bool VerifyPassword(string plainPassword, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword) || string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
            }
            catch
            {
                return false;
            }
        }

        public static void ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña no puede estar vacía");

            if (password.Length < MinLength)
                throw new ArgumentException($"La contraseña debe tener al menos {MinLength} caracteres");

            bool hasUpperCase = false;
            bool hasLowerCase = false;
            bool hasDigit = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c)) hasUpperCase = true;
                if (char.IsLower(c)) hasLowerCase = true;
                if (char.IsDigit(c)) hasDigit = true;
            }

            if (!hasUpperCase)
                throw new ArgumentException("La contraseña debe contener al menos una letra mayúscula");
            
            if (!hasLowerCase)
                throw new ArgumentException("La contraseña debe contener al menos una letra minúscula");
            
            if (!hasDigit)
                throw new ArgumentException("La contraseña debe contener al menos un dígito");
        }
    }
}