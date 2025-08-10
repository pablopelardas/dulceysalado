using System;
using DistriCatalogoAPI.Domain.Common;

namespace DistriCatalogoAPI.Domain.Entities
{
    public class Agrupacion : BaseEntity
    {
        public int Id { get; private set; }
        public int Codigo { get; private set; } // Valor que viene de Grupo3
        public string Nombre { get; private set; }
        public string? Descripcion { get; private set; }
        public bool Activa { get; private set; }
        public int EmpresaPrincipalId { get; private set; }
        public int Tipo { get; private set; } // 1=Grupo1(Novedades/Ofertas), 2=Grupo2(Futuro), 3=Grupo3(Actual)

        // Navigation
        public virtual Company EmpresaPrincipal { get; private set; }

        protected Agrupacion() { }

        // Constructor para crear nueva agrupación desde sincronización
        public static Agrupacion CreateFromSync(
            int codigo,
            int empresaPrincipalId,
            string? nombre = null,
            string? descripcion = null,
            int tipo = 3)
        {
            if (codigo <= 0)
                throw new ArgumentException("El código de agrupación debe ser mayor a 0");

            if (empresaPrincipalId <= 0)
                throw new ArgumentException("El ID de empresa principal debe ser mayor a 0");

            if (tipo < 1 || tipo > 3)
                throw new ArgumentException("El tipo debe ser 1 (Grupo1), 2 (Grupo2) o 3 (Grupo3)");

            return new Agrupacion
            {
                Codigo = codigo,
                Nombre = nombre ?? $"Agrupación {codigo}",
                Descripcion = descripcion,
                Activa = true,
                EmpresaPrincipalId = empresaPrincipalId,
                Tipo = tipo,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        // Constructor para crear agrupación manualmente
        public static Agrupacion Create(
            int codigo,
            string nombre,
            int empresaPrincipalId,
            string? descripcion = null,
            bool activa = true,
            int tipo = 3)
        {
            if (codigo <= 0)
                throw new ArgumentException("El código de agrupación debe ser mayor a 0");

            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la agrupación es requerido");

            if (empresaPrincipalId <= 0)
                throw new ArgumentException("El ID de empresa principal debe ser mayor a 0");

            if (tipo < 1 || tipo > 3)
                throw new ArgumentException("El tipo debe ser 1 (Grupo1), 2 (Grupo2) o 3 (Grupo3)");

            return new Agrupacion
            {
                Codigo = codigo,
                Nombre = nombre.Trim(),
                Descripcion = descripcion?.Trim(),
                Activa = activa,
                EmpresaPrincipalId = empresaPrincipalId,
                Tipo = tipo,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        // Método para actualizar información
        public void Update(
            string? nombre = null,
            string? descripcion = null,
            bool? activa = null)
        {
            if (nombre != null)
            {
                if (string.IsNullOrWhiteSpace(nombre))
                    throw new ArgumentException("El nombre de la agrupación no puede estar vacío");
                Nombre = nombre.Trim();
            }

            if (descripcion != null)
                Descripcion = descripcion.Trim();

            if (activa.HasValue)
                Activa = activa.Value;

            UpdatedAt = DateTime.UtcNow;
        }

        // Método para desactivar
        public void Deactivate()
        {
            Activa = false;
            UpdatedAt = DateTime.UtcNow;
        }

        // Método para activar
        public void Activate()
        {
            Activa = true;
            UpdatedAt = DateTime.UtcNow;
        }

        // Métodos de consulta
        public bool IsActive()
        {
            return Activa;
        }

        public bool BelongsToCompany(int companyId)
        {
            return EmpresaPrincipalId == companyId;
        }

        // Métodos para trabajar con tipos
        public bool IsGrupo1()
        {
            return Tipo == 1;
        }

        public bool IsGrupo2()
        {
            return Tipo == 2;
        }

        public bool IsGrupo3()
        {
            return Tipo == 3;
        }

        public void SetTipo(int tipo)
        {
            if (tipo < 1 || tipo > 3)
                throw new ArgumentException("El tipo debe ser 1 (Grupo1), 2 (Grupo2) o 3 (Grupo3)");
            
            Tipo = tipo;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}