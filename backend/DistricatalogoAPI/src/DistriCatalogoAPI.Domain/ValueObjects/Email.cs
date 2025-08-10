using System;
using System.Text.RegularExpressions;

namespace DistriCatalogoAPI.Domain.ValueObjects
{
    public class Email
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string Value { get; }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El email no puede estar vacío", nameof(value));

            if (!EmailRegex.IsMatch(value))
                throw new ArgumentException("Formato de email inválido", nameof(value));

            Value = value.ToLowerInvariant();
        }

        public override string ToString() => Value;

        public override bool Equals(object? obj)
        {
            if (obj is Email other)
                return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
            
            return false;
        }

        public override int GetHashCode() => Value.GetHashCode();

        public static implicit operator string(Email email) => email?.Value;
        
        public static explicit operator Email(string value) => new Email(value);
    }
}