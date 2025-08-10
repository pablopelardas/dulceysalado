using System;
using DistriCatalogoAPI.Domain.Common;

namespace DistriCatalogoAPI.Domain.Entities
{
    public class Company : BaseEntity
    {
        public int Id { get; private set; }
        public string Codigo { get; private set; }
        public string Nombre { get; private set; }
        public string RazonSocial { get; private set; }
        public string Cuit { get; private set; }
        public string Telefono { get; private set; }
        public string Email { get; private set; }
        public string Direccion { get; private set; }
        public string TipoEmpresa { get; private set; }
        public int? EmpresaPrincipalId { get; private set; }
        public string LogoUrl { get; private set; }
        public string ColoresTema { get; private set; }
        public string FaviconUrl { get; private set; }
        public string DominioPersonalizado { get; private set; }
        public string UrlWhatsapp { get; private set; }
        public string UrlFacebook { get; private set; }
        public string UrlInstagram { get; private set; }
        public bool MostrarPrecios { get; private set; }
        public bool MostrarStock { get; private set; }
        public bool PermitirPedidos { get; private set; }
        public int ProductosPorPagina { get; private set; }
        public bool PuedeAgregarProductos { get; private set; }
        public bool PuedeAgregarCategorias { get; private set; }
        public bool Activa { get; private set; }
        public DateTime? FechaVencimiento { get; private set; }
        public string Plan { get; private set; }
        public int? ListaPrecioPredeterminadaId { get; private set; }

        // Private constructor for EF Core
        private Company() { }

        public Company(string codigo, string nombre, string tipoEmpresa, int? empresaPrincipalId = null)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                throw new ArgumentException("Company code is required", nameof(codigo));
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("Company name is required", nameof(nombre));
            if (string.IsNullOrWhiteSpace(tipoEmpresa))
                throw new ArgumentException("Company type is required", nameof(tipoEmpresa));

            Codigo = codigo.Trim();
            Nombre = nombre.Trim();
            TipoEmpresa = tipoEmpresa.ToLower();
            EmpresaPrincipalId = empresaPrincipalId;
            Activa = true;
            ProductosPorPagina = 20; // Default value
            
            // Set default permissions based on company type
            SetDefaultPermissions();
        }

        public bool IsPrincipal => TipoEmpresa?.ToLower() == "principal";
        public bool IsCliente => TipoEmpresa?.ToLower() == "cliente";

        public void UpdateBasicInfo(string nombre, string razonSocial, string cuit, string telefono, string email, string direccion)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("Company name is required", nameof(nombre));

            Nombre = nombre.Trim();
            RazonSocial = razonSocial?.Trim();
            Cuit = cuit?.Trim();
            Telefono = telefono?.Trim();
            Email = email?.Trim();
            Direccion = direccion?.Trim();
            MarkAsUpdated();
        }

        public void UpdateCatalogSettings(bool mostrarPrecios, bool mostrarStock, bool permitirPedidos, int productosPorPagina)
        {
            if (productosPorPagina <= 0)
                throw new ArgumentException("Products per page must be greater than 0", nameof(productosPorPagina));

            MostrarPrecios = mostrarPrecios;
            MostrarStock = mostrarStock;
            PermitirPedidos = permitirPedidos;
            ProductosPorPagina = productosPorPagina;
            MarkAsUpdated();
        }

        public void UpdateBranding(string logoUrl, string coloresTema, string faviconUrl, string dominioPersonalizado)
        {
            LogoUrl = logoUrl?.Trim();
            ColoresTema = coloresTema?.Trim();
            FaviconUrl = faviconUrl?.Trim();
            DominioPersonalizado = dominioPersonalizado?.Trim();
            MarkAsUpdated();
        }

        public void UpdateSocialMedia(string urlWhatsapp, string urlFacebook, string urlInstagram)
        {
            UrlWhatsapp = urlWhatsapp?.Trim();
            UrlFacebook = urlFacebook?.Trim();
            UrlInstagram = urlInstagram?.Trim();
            MarkAsUpdated();
        }

        public void UpdatePermissions(bool puedeAgregarProductos, bool puedeAgregarCategorias)
        {
            // Both principal and client companies can manage their own products and categories
            PuedeAgregarProductos = puedeAgregarProductos;
            PuedeAgregarCategorias = puedeAgregarCategorias;
            MarkAsUpdated();
        }

        public void UpdatePlanAndExpiration(string plan, DateTime? fechaVencimiento)
        {
            if (string.IsNullOrWhiteSpace(plan))
                throw new ArgumentException("Plan is required", nameof(plan));

            Plan = plan.Trim().ToLower();
            FechaVencimiento = fechaVencimiento;
            MarkAsUpdated();
        }

        public void SetListaPrecioPredeterminada(int? listaPrecioId)
        {
            ListaPrecioPredeterminadaId = listaPrecioId;
            MarkAsUpdated();
        }

        public void Deactivate()
        {
            if (IsPrincipal)
                throw new InvalidOperationException("Principal company cannot be deactivated");

            Activa = false;
            MarkAsUpdated();
        }

        public void Reactivate()
        {
            Activa = true;
            MarkAsUpdated();
        }

        public bool CanManageCompany(int targetCompanyId, int requestingCompanyId)
        {
            // Principal companies can manage any client company
            if (IsPrincipal && targetCompanyId != Id)
                return true;

            // Companies can manage themselves
            return targetCompanyId == requestingCompanyId;
        }

        /// <summary>
        /// Verifica si esta empresa principal puede gestionar los productos de una empresa cliente específica
        /// </summary>
        public bool CanManageClientCompanyProducts(Company clientCompany)
        {
            if (!IsPrincipal)
                return false;

            if (!clientCompany.IsCliente)
                return false;

            return clientCompany.EmpresaPrincipalId == Id;
        }

        public bool IsExpired => FechaVencimiento.HasValue && FechaVencimiento.Value < DateTime.UtcNow;

        private void SetDefaultPermissions()
        {
            if (IsPrincipal)
            {
                PuedeAgregarProductos = true;
                PuedeAgregarCategorias = true;
                Plan = "premium";
            }
            else
            {
                PuedeAgregarProductos = false;
                PuedeAgregarCategorias = false;
                Plan = "basico";
                MostrarPrecios = true;
                MostrarStock = false;
                PermitirPedidos = false;
            }
        }
    }
}