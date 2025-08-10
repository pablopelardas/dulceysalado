using System;
using System.Collections.Generic;
using System.Linq;
using DistriCatalogoAPI.Domain.Common;

namespace DistriCatalogoAPI.Domain.ValueObjects
{
    public class SessionState : ValueObject
    {
        public string Value { get; private set; }

        // Constructor sin validación compleja para evitar problemas de inicialización
        private SessionState(string value)
        {
            Value = value?.ToLower() ?? throw new ArgumentNullException(nameof(value));
        }

        // Estados predefinidos usando el constructor simple
        public static SessionState Iniciada { get; } = new SessionState("iniciada");
        public static SessionState Procesando { get; } = new SessionState("procesando");
        public static SessionState Completada { get; } = new SessionState("completada");
        public static SessionState Error { get; } = new SessionState("error");
        public static SessionState Cancelada { get; } = new SessionState("cancelada");

        private static readonly HashSet<string> ValidStates = new()
        {
            "iniciada", "procesando", "completada", "error", "cancelada"
        };

        // Factory method con validación
        public static SessionState From(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El estado de sesión no puede estar vacío");

            value = value.ToLower();

            if (!ValidStates.Contains(value))
                throw new ArgumentException($"Estado de sesión inválido: {value}");

            return new SessionState(value);
        }

        public static implicit operator string(SessionState state)
        {
            return state.Value;
        }

        public override string ToString()
        {
            return Value;
        }

        public bool CanTransitionTo(SessionState newState)
        {
            return (Value, newState.Value) switch
            {
                ("iniciada", "procesando") => true,
                ("iniciada", "completada") => true,
                ("iniciada", "error") => true,
                ("iniciada", "cancelada") => true,
                ("procesando", "completada") => true,
                ("procesando", "error") => true,
                ("procesando", "cancelada") => true,
                _ => false
            };
        }

        public bool IsTerminal()
        {
            return Value == "completada" || Value == "error" || Value == "cancelada";
        }

        public bool IsActive()
        {
            return Value == "iniciada" || Value == "procesando";
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}