using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Infrastructure.Models;

public partial class Empresa
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? RazonSocial { get; set; }

    public string? Cuit { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? Direccion { get; set; }

    public string? TipoEmpresa { get; set; }

    public int? EmpresaPrincipalId { get; set; }

    public string? LogoUrl { get; set; }

    public string? ColoresTema { get; set; }

    public string? FaviconUrl { get; set; }

    public string? DominioPersonalizado { get; set; }

    public string? UrlWhatsapp { get; set; }

    public string? UrlFacebook { get; set; }

    public string? UrlInstagram { get; set; }

    public bool? MostrarPrecios { get; set; }

    public bool? MostrarStock { get; set; }

    public bool? PermitirPedidos { get; set; }

    public int? ProductosPorPagina { get; set; }

    public bool? PuedeAgregarProductos { get; set; }

    public bool? PuedeAgregarCategorias { get; set; }

    public bool? Activa { get; set; }

    public DateTime? FechaVencimiento { get; set; }

    public string? Plan { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? ListaPrecioPredeterminadaId { get; set; }

    public virtual ListasPrecio? ListaPrecioPredeterminada { get; set; }

    public virtual ICollection<CategoriasBase> CategoriasBases { get; set; } = new List<CategoriasBase>();

    public virtual ICollection<CategoriasEmpresa> CategoriasEmpresas { get; set; } = new List<CategoriasEmpresa>();

    public virtual Empresa? EmpresaPrincipal { get; set; }

    public virtual ICollection<Empresa> InverseEmpresaPrincipal { get; set; } = new List<Empresa>();

    public virtual ICollection<ProductosBase> ProductosBases { get; set; } = new List<ProductosBase>();

    public virtual ICollection<ProductosEmpresa> ProductosEmpresas { get; set; } = new List<ProductosEmpresa>();

    public virtual ICollection<SyncLog> SyncLogs { get; set; } = new List<SyncLog>();

    public virtual ICollection<SyncSession> SyncSessions { get; set; } = new List<SyncSession>();

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

    public virtual ICollection<ProductosEmpresaPrecio> ProductosEmpresaPrecios { get; set; } = new List<ProductosEmpresaPrecio>();

    public virtual ICollection<Agrupaciones> Agrupaciones { get; set; } = new List<Agrupaciones>();

    public virtual ICollection<EmpresasAgrupacionesVisible> EmpresasAgrupacionesVisibles { get; set; } = new List<EmpresasAgrupacionesVisible>();

    public virtual ICollection<EmpresasNovedad> EmpresasNovedades { get; set; } = new List<EmpresasNovedad>();

    public virtual ICollection<EmpresasOferta> EmpresasOfertas { get; set; } = new List<EmpresasOferta>();
}
