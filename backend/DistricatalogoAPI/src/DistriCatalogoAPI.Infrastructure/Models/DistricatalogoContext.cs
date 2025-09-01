using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DistriCatalogoAPI.Infrastructure.Models;

public partial class DistricatalogoContext : DbContext
{
    public DistricatalogoContext()
    {
    }

    public DistricatalogoContext(DbContextOptions<DistricatalogoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CategoriasBase> CategoriasBases { get; set; }

    public virtual DbSet<CategoriasEmpresa> CategoriasEmpresas { get; set; }

    public virtual DbSet<ConfiguracionSistema> ConfiguracionSistemas { get; set; }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<ProductoImagene> ProductoImagenes { get; set; }

    public virtual DbSet<ProductosBase> ProductosBases { get; set; }

    public virtual DbSet<ProductosEmpresa> ProductosEmpresas { get; set; }
    
    public virtual DbSet<DistriCatalogoAPI.Domain.Entities.Cliente> Clientes { get; set; }
    
    public virtual DbSet<DistriCatalogoAPI.Domain.Entities.ClienteRefreshToken> ClienteRefreshTokens { get; set; }
    
    public virtual DbSet<DistriCatalogoAPI.Domain.Entities.ClienteLoginHistory> ClienteLoginHistories { get; set; }
    
    public virtual DbSet<DistriCatalogoAPI.Domain.Entities.CustomerSyncSession> CustomerSyncSessions { get; set; }

    public virtual DbSet<SyncLog> SyncLogs { get; set; }

    public virtual DbSet<SyncSession> SyncSessions { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }
    
    public virtual DbSet<DistriCatalogoAPI.Domain.Entities.UserNotificationPreferences> UserNotificationPreferences { get; set; }

    public virtual DbSet<VistaCatalogoEmpresa> VistaCatalogoEmpresas { get; set; }

    public virtual DbSet<VistaCategoriasEmpresa> VistaCategoriasEmpresas { get; set; }

    public virtual DbSet<ListasPrecio> ListasPrecios { get; set; }

    public virtual DbSet<ProductosBasePrecio> ProductosBasePrecios { get; set; }

    public virtual DbSet<ProductosEmpresaPrecio> ProductosEmpresaPrecios { get; set; }

    public virtual DbSet<VistaProductosPreciosEmpresa> VistaProductosPreciosEmpresas { get; set; }

    public virtual DbSet<Agrupaciones> Agrupaciones { get; set; }

    public virtual DbSet<EmpresasAgrupacionesVisible> EmpresasAgrupacionesVisibles { get; set; }

    public virtual DbSet<EmpresasNovedad> EmpresasNovedades { get; set; }

    public virtual DbSet<EmpresasOferta> EmpresasOfertas { get; set; }

    public virtual DbSet<ProductosBaseStock> ProductosBaseStocks { get; set; }

    public virtual DbSet<DistriCatalogoAPI.Domain.Entities.FeatureDefinition> FeatureDefinitions { get; set; }

    public virtual DbSet<DistriCatalogoAPI.Domain.Entities.EmpresaFeature> EmpresaFeatures { get; set; }

    public virtual DbSet<DistriCatalogoAPI.Domain.Entities.Pedido> Pedidos { get; set; }

    public virtual DbSet<DistriCatalogoAPI.Domain.Entities.PedidoItem> PedidoItems { get; set; }
    
    public virtual DbSet<DistriCatalogoAPI.Domain.Entities.CorreccionToken> CorrecionTokens { get; set; }
    
    public virtual DbSet<DistriCatalogoAPI.Domain.Entities.DeliverySettings> DeliverySettings { get; set; }
    
    public virtual DbSet<DistriCatalogoAPI.Domain.Entities.DeliverySchedule> DeliverySchedules { get; set; }
    
    public virtual DbSet<DistriCatalogoAPI.Domain.Entities.DeliverySlot> DeliverySlots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoriasBase>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("categorias_base");

            entity.HasIndex(e => e.CodigoRubro, "codigo_rubro").IsUnique();

            entity.HasIndex(e => e.CreatedByEmpresaId, "created_by_empresa_id");

            entity.HasIndex(e => e.CodigoRubro, "idx_codigo_rubro");

            entity.HasIndex(e => new { e.Visible, e.Orden }, "idx_visible_orden");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CodigoRubro).HasColumnName("codigo_rubro");
            entity.Property(e => e.Color)
                .HasMaxLength(7)
                .HasDefaultValueSql("'#4A90E2'")
                .HasColumnName("color");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedByEmpresaId).HasColumnName("created_by_empresa_id");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.Icono)
                .HasMaxLength(50)
                .HasDefaultValueSql("'?'")
                .HasColumnName("icono");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Orden)
                .HasDefaultValueSql("'0'")
                .HasColumnName("orden");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.Visible)
                .HasDefaultValueSql("'1'")
                .HasColumnName("visible");

            entity.HasOne(d => d.CreatedByEmpresa).WithMany(p => p.CategoriasBases)
                .HasForeignKey(d => d.CreatedByEmpresaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("categorias_base_ibfk_1");
        });

        modelBuilder.Entity<CategoriasEmpresa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("categorias_empresa");

            entity.HasIndex(e => new { e.EmpresaId, e.CodigoRubro }, "idx_empresa_rubro");

            entity.HasIndex(e => new { e.Visible, e.Orden }, "idx_visible_orden");

            entity.HasIndex(e => new { e.EmpresaId, e.CodigoRubro }, "unique_empresa_rubro").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CodigoRubro).HasColumnName("codigo_rubro");
            entity.Property(e => e.Color)
                .HasMaxLength(7)
                .HasDefaultValueSql("'#4A90E2'")
                .HasColumnName("color");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.Icono)
                .HasMaxLength(50)
                .HasDefaultValueSql("'?'")
                .HasColumnName("icono");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Orden)
                .HasDefaultValueSql("'0'")
                .HasColumnName("orden");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.Visible)
                .HasDefaultValueSql("'1'")
                .HasColumnName("visible");

            entity.HasOne(d => d.Empresa).WithMany(p => p.CategoriasEmpresas)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("categorias_empresa_ibfk_1");
        });

        modelBuilder.Entity<ConfiguracionSistema>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("configuracion_sistema");

            entity.HasIndex(e => e.Clave, "clave").IsUnique();

            entity.HasIndex(e => e.Clave, "idx_clave");

            entity.HasIndex(e => e.Publico, "idx_publico");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Clave)
                .HasMaxLength(100)
                .HasColumnName("clave");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.Publico)
                .HasDefaultValueSql("'0'")
                .HasColumnName("publico");
            entity.Property(e => e.Tipo)
                .HasDefaultValueSql("'string'")
                .HasColumnType("enum('string','number','boolean','json')")
                .HasColumnName("tipo");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.Valor)
                .HasColumnType("text")
                .HasColumnName("valor");
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("empresas");

            entity.HasIndex(e => e.Codigo, "codigo").IsUnique();

            entity.HasIndex(e => e.EmpresaPrincipalId, "empresa_principal_id");

            entity.HasIndex(e => e.Activa, "idx_activa");

            entity.HasIndex(e => e.Codigo, "idx_codigo");

            entity.HasIndex(e => e.DominioPersonalizado, "idx_dominio");

            entity.HasIndex(e => e.TipoEmpresa, "idx_tipo_empresa");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activa)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activa");
            entity.Property(e => e.Codigo)
                .HasMaxLength(50)
                .HasColumnName("codigo");
            entity.Property(e => e.ColoresTema)
                .HasColumnType("json")
                .HasColumnName("colores_tema");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Cuit)
                .HasMaxLength(15)
                .HasColumnName("cuit");
            entity.Property(e => e.Direccion)
                .HasColumnType("text")
                .HasColumnName("direccion");
            entity.Property(e => e.DominioPersonalizado)
                .HasMaxLength(100)
                .HasColumnName("dominio_personalizado");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.EmpresaPrincipalId).HasColumnName("empresa_principal_id");
            entity.Property(e => e.FaviconUrl)
                .HasMaxLength(500)
                .HasColumnName("favicon_url");
            entity.Property(e => e.FechaVencimiento)
                .HasColumnType("date")
                .HasColumnName("fecha_vencimiento");
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(500)
                .HasColumnName("logo_url");
            entity.Property(e => e.MostrarPrecios)
                .HasDefaultValueSql("'1'")
                .HasColumnName("mostrar_precios");
            entity.Property(e => e.MostrarStock)
                .HasDefaultValueSql("'0'")
                .HasColumnName("mostrar_stock");
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .HasColumnName("nombre");
            entity.Property(e => e.PermitirPedidos)
                .HasDefaultValueSql("'0'")
                .HasColumnName("permitir_pedidos");
            entity.Property(e => e.Plan)
                .HasDefaultValueSql("'basico'")
                .HasColumnType("enum('basico','premium','enterprise')")
                .HasColumnName("plan");
            entity.Property(e => e.ProductosPorPagina)
                .HasDefaultValueSql("'20'")
                .HasColumnName("productos_por_pagina");
            entity.Property(e => e.PuedeAgregarCategorias)
                .HasDefaultValueSql("'0'")
                .HasColumnName("puede_agregar_categorias");
            entity.Property(e => e.PuedeAgregarProductos)
                .HasDefaultValueSql("'0'")
                .HasColumnName("puede_agregar_productos");
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(250)
                .HasColumnName("razon_social");
            entity.Property(e => e.Telefono)
                .HasMaxLength(50)
                .HasColumnName("telefono");
            entity.Property(e => e.TipoEmpresa)
                .HasDefaultValueSql("'cliente'")
                .HasColumnType("enum('principal','cliente')")
                .HasColumnName("tipo_empresa");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UrlFacebook)
                .HasMaxLength(200)
                .HasColumnName("url_facebook");
            entity.Property(e => e.UrlInstagram)
                .HasMaxLength(200)
                .HasColumnName("url_instagram");
            entity.Property(e => e.UrlWhatsapp)
                .HasMaxLength(200)
                .HasColumnName("url_whatsapp");
            entity.Property(e => e.ListaPrecioPredeterminadaId)
                .HasColumnName("lista_precio_predeterminada_id");

            entity.HasOne(d => d.EmpresaPrincipal).WithMany(p => p.InverseEmpresaPrincipal)
                .HasForeignKey(d => d.EmpresaPrincipalId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("empresas_ibfk_1");

            entity.HasOne(d => d.ListaPrecioPredeterminada).WithMany()
                .HasForeignKey(d => d.ListaPrecioPredeterminadaId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_empresas_lista_precio_predeterminada");
        });

        modelBuilder.Entity<ProductoImagene>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("producto_imagenes");

            entity.HasIndex(e => e.EmpresaId, "idx_empresa_id");

            entity.HasIndex(e => e.EsPrincipal, "idx_es_principal");

            entity.HasIndex(e => new { e.TipoImagen, e.Orden }, "idx_tipo_orden");

            entity.HasIndex(e => new { e.TipoProducto, e.ProductoId }, "idx_tipo_producto_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AltText)
                .HasMaxLength(255)
                .HasColumnName("alt_text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.EsPrincipal)
                .HasDefaultValueSql("'0'")
                .HasColumnName("es_principal");
            entity.Property(e => e.HeightPx).HasColumnName("height_px");
            entity.Property(e => e.Orden)
                .HasDefaultValueSql("'0'")
                .HasColumnName("orden");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.SizeBytes).HasColumnName("size_bytes");
            entity.Property(e => e.TipoImagen)
                .HasDefaultValueSql("'galeria'")
                .HasColumnType("enum('principal','galeria','miniatura')")
                .HasColumnName("tipo_imagen");
            entity.Property(e => e.TipoProducto)
                .HasColumnType("enum('base','empresa')")
                .HasColumnName("tipo_producto");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UrlImagen)
                .HasMaxLength(500)
                .HasColumnName("url_imagen");
            entity.Property(e => e.WidthPx).HasColumnName("width_px");
        });

        modelBuilder.Entity<ProductosBase>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("productos_base");

            entity.HasIndex(e => e.AdministradoPorEmpresaId, "administrado_por_empresa_id");

            entity.HasIndex(e => e.Codigo, "codigo").IsUnique();

            entity.HasIndex(e => e.ActualizadoGecom, "idx_actualizado_gecom");

            entity.HasIndex(e => new { e.Descripcion, e.DescripcionCorta, e.DescripcionLarga, e.Tags, e.Marca }, "idx_busqueda");

            entity.HasIndex(e => e.Codigo, "idx_codigo");

            entity.HasIndex(e => e.CodigoBarras, "idx_codigo_barras");

            entity.HasIndex(e => e.CodigoRubro, "idx_codigo_rubro");

            entity.HasIndex(e => e.Destacado, "idx_destacado");

            entity.HasIndex(e => e.Marca, "idx_marca");

            // entity.HasIndex(e => e.Precio, "idx_precio"); // ELIMINADO: campo precio no existe más

            entity.HasIndex(e => new { e.Visible, e.CodigoRubro }, "idx_visible_categoria");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActualizadoGecom)
                .HasColumnType("timestamp")
                .HasColumnName("actualizado_gecom");
            entity.Property(e => e.AdministradoPorEmpresaId).HasColumnName("administrado_por_empresa_id");
            entity.Property(e => e.Codigo).HasColumnName("codigo");
            entity.Property(e => e.CodigoBarras)
                .HasMaxLength(100)
                .HasColumnName("codigo_barras");
            entity.Property(e => e.CodigoRubro).HasColumnName("codigo_rubro");
            entity.Property(e => e.CodigoUbicacion)
                .HasMaxLength(50)
                .HasColumnName("codigo_ubicacion");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .HasColumnName("descripcion");
            entity.Property(e => e.DescripcionCorta)
                .HasMaxLength(200)
                .HasColumnName("descripcion_corta");
            entity.Property(e => e.DescripcionLarga)
                .HasColumnType("text")
                .HasColumnName("descripcion_larga");
            entity.Property(e => e.Destacado)
                .HasDefaultValueSql("'0'")
                .HasColumnName("destacado");
            entity.Property(e => e.Disponible)
                .HasMaxLength(1)
                .HasDefaultValueSql("'S'")
                .IsFixedLength()
                .HasColumnName("disponible");
            // entity.Property(e => e.Existencia) - ELIMINADO: Campo movido a productos_base_stock
            //     .HasPrecision(8)
            //     .HasDefaultValueSql("'0.00'")
            //     .HasColumnName("existencia");
            entity.Property(e => e.FechaAlta)
                .HasColumnType("date")
                .HasColumnName("fecha_alta");
            entity.Property(e => e.FechaModi)
                .HasColumnType("date")
                .HasColumnName("fecha_modi");
            entity.Property(e => e.Grupo1).HasColumnName("grupo1");
            entity.Property(e => e.Grupo2).HasColumnName("grupo2");
            entity.Property(e => e.Grupo3).HasColumnName("grupo3");
            entity.Property(e => e.ImagenAlt)
                .HasMaxLength(255)
                .HasColumnName("imagen_alt");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(500)
                .HasColumnName("imagen_url");
            entity.Property(e => e.Imputable)
                .HasMaxLength(1)
                .HasDefaultValueSql("'S'")
                .IsFixedLength()
                .HasColumnName("imputable");
            entity.Property(e => e.Marca)
                .HasMaxLength(100)
                .HasColumnName("marca");
            entity.Property(e => e.OrdenCategoria)
                .HasDefaultValueSql("'0'")
                .HasColumnName("orden_categoria");
            // entity.Property(e => e.Precio) // ELIMINADO: campo precio no existe más
            //     .HasPrecision(10, 3)
            //     .HasDefaultValueSql("'0.000'")
            //     .HasColumnName("precio");
            entity.Property(e => e.Tags)
                .HasMaxLength(500)
                .HasColumnName("tags");
            entity.Property(e => e.UnidadMedida)
                .HasMaxLength(20)
                .HasDefaultValueSql("'UN'")
                .HasColumnName("unidad_medida");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.Visible)
                .HasDefaultValueSql("'1'")
                .HasColumnName("visible");

            entity.HasOne(d => d.AdministradoPorEmpresa).WithMany(p => p.ProductosBases)
                .HasForeignKey(d => d.AdministradoPorEmpresaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("productos_base_ibfk_2");

            entity.HasOne(d => d.CodigoRubroNavigation).WithMany(p => p.ProductosBases)
                .HasPrincipalKey(p => p.CodigoRubro)
                .HasForeignKey(d => d.CodigoRubro)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("productos_base_ibfk_1");
        });

        modelBuilder.Entity<ProductosEmpresa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("productos_empresa");

            entity.HasIndex(e => new { e.Descripcion, e.DescripcionCorta, e.DescripcionLarga, e.Tags, e.Marca }, "idx_busqueda");

            entity.HasIndex(e => e.CodigoRubro, "idx_codigo_rubro");

            entity.HasIndex(e => e.Destacado, "idx_destacado");

            entity.HasIndex(e => new { e.EmpresaId, e.Codigo }, "idx_empresa_codigo");

            // entity.HasIndex(e => e.Precio, "idx_precio"); // ELIMINADO: campo precio no existe más

            entity.HasIndex(e => new { e.Visible, e.CodigoRubro }, "idx_visible_categoria");

            entity.HasIndex(e => new { e.EmpresaId, e.Codigo }, "unique_empresa_codigo").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Codigo).HasColumnName("codigo");
            entity.Property(e => e.CodigoBarras)
                .HasMaxLength(100)
                .HasColumnName("codigo_barras");
            entity.Property(e => e.CodigoRubro).HasColumnName("codigo_rubro");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .HasColumnName("descripcion");
            entity.Property(e => e.DescripcionCorta)
                .HasMaxLength(200)
                .HasColumnName("descripcion_corta");
            entity.Property(e => e.DescripcionLarga)
                .HasColumnType("text")
                .HasColumnName("descripcion_larga");
            entity.Property(e => e.Destacado)
                .HasDefaultValueSql("'0'")
                .HasColumnName("destacado");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.Existencia)
                .HasPrecision(8)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("existencia");
            entity.Property(e => e.ImagenAlt)
                .HasMaxLength(255)
                .HasColumnName("imagen_alt");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(500)
                .HasColumnName("imagen_url");
            entity.Property(e => e.Marca)
                .HasMaxLength(100)
                .HasColumnName("marca");
            entity.Property(e => e.OrdenCategoria)
                .HasDefaultValueSql("'0'")
                .HasColumnName("orden_categoria");
            // entity.Property(e => e.Precio) // ELIMINADO: campo precio no existe más
            //     .HasPrecision(10, 3)
            //     .HasDefaultValueSql("'0.000'")
            //     .HasColumnName("precio");
            entity.Property(e => e.Tags)
                .HasMaxLength(500)
                .HasColumnName("tags");
            entity.Property(e => e.UnidadMedida)
                .HasMaxLength(20)
                .HasDefaultValueSql("'UN'")
                .HasColumnName("unidad_medida");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.Visible)
                .HasDefaultValueSql("'1'")
                .HasColumnName("visible");

            entity.HasOne(d => d.Empresa).WithMany(p => p.ProductosEmpresas)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("productos_empresa_ibfk_1");
        });

        modelBuilder.Entity<SyncLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("sync_logs");

            entity.HasIndex(e => new { e.EmpresaPrincipalId, e.FechaProcesamiento }, "idx_empresa_fecha");

            entity.HasIndex(e => e.Estado, "idx_estado");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArchivoNombre)
                .HasMaxLength(255)
                .HasColumnName("archivo_nombre");
            entity.Property(e => e.DetallesErrores)
                .HasColumnType("text")
                .HasColumnName("detalles_errores");
            entity.Property(e => e.EmpresaPrincipalId).HasColumnName("empresa_principal_id");
            entity.Property(e => e.Errores)
                .HasDefaultValueSql("'0'")
                .HasColumnName("errores");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'exitoso'")
                .HasColumnType("enum('exitoso','con_errores','fallido')")
                .HasColumnName("estado");
            entity.Property(e => e.FechaProcesamiento)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("fecha_procesamiento");
            entity.Property(e => e.ProductosActualizados)
                .HasDefaultValueSql("'0'")
                .HasColumnName("productos_actualizados");
            entity.Property(e => e.ProductosNuevos)
                .HasDefaultValueSql("'0'")
                .HasColumnName("productos_nuevos");
            entity.Property(e => e.TiempoProcesamientoMs).HasColumnName("tiempo_procesamiento_ms");
            entity.Property(e => e.UsuarioProceso)
                .HasMaxLength(100)
                .HasColumnName("usuario_proceso");

            entity.HasOne(d => d.EmpresaPrincipal).WithMany(p => p.SyncLogs)
                .HasForeignKey(d => d.EmpresaPrincipalId)
                .HasConstraintName("sync_logs_ibfk_1");
        });

        modelBuilder.Entity<SyncSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("sync_sessions");

            entity.HasIndex(e => e.CreatedAt, "idx_created_at");

            entity.HasIndex(e => e.EmpresaPrincipalId, "idx_empresa_principal");

            entity.HasIndex(e => e.ListaPrecioId, "idx_lista_precio_id");

            entity.HasIndex(e => e.Estado, "idx_estado");

            entity.HasIndex(e => e.FechaInicio, "idx_fecha_inicio");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.EmpresaPrincipalId).HasColumnName("empresa_principal_id");
            entity.Property(e => e.ListaPrecioId).HasColumnName("lista_precio_id");
            entity.Property(e => e.ErroresDetalle)
                .HasColumnType("json")
                .HasColumnName("errores_detalle");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'iniciada'")
                .HasColumnType("enum('iniciada','procesando','completada','error','cancelada')")
                .HasColumnName("estado");
            entity.Property(e => e.FechaFin)
                .HasColumnType("timestamp")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("fecha_inicio");
            entity.Property(e => e.IpOrigen)
                .HasMaxLength(45)
                .HasColumnName("ip_origen");
            entity.Property(e => e.LotesProcesados)
                .HasDefaultValueSql("'0'")
                .HasColumnName("lotes_procesados");
            entity.Property(e => e.Metricas)
                .HasColumnType("json")
                .HasColumnName("metricas");
            entity.Property(e => e.ProductosActualizados)
                .HasDefaultValueSql("'0'")
                .HasColumnName("productos_actualizados");
            entity.Property(e => e.ProductosErrores)
                .HasDefaultValueSql("'0'")
                .HasColumnName("productos_errores");
            entity.Property(e => e.ProductosNuevos)
                .HasDefaultValueSql("'0'")
                .HasColumnName("productos_nuevos");
            entity.Property(e => e.ProductosTotales)
                .HasDefaultValueSql("'0'")
                .HasColumnName("productos_totales");
            entity.Property(e => e.TiempoTotalMs).HasColumnName("tiempo_total_ms");
            entity.Property(e => e.TotalLotesEsperados).HasColumnName("total_lotes_esperados");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UsuarioProceso)
                .HasMaxLength(100)
                .HasColumnName("usuario_proceso");

            entity.HasOne(d => d.EmpresaPrincipal).WithMany(p => p.SyncSessions)
                .HasForeignKey(d => d.EmpresaPrincipalId)
                .HasConstraintName("sync_sessions_ibfk_1");

            entity.HasOne(d => d.ListaPrecio).WithMany()
                .HasForeignKey(d => d.ListaPrecioId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("sync_sessions_ibfk_2");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => e.Activo, "idx_activo");

            entity.HasIndex(e => new { e.EmpresaId, e.Email }, "idx_empresa_email");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .HasColumnName("apellido");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.PuedeGestionarCategoriasBase)
                .HasDefaultValueSql("'0'")
                .HasColumnName("puede_gestionar_categorias_base");
            entity.Property(e => e.PuedeGestionarCategoriasEmpresa)
                .HasDefaultValueSql("'0'")
                .HasColumnName("puede_gestionar_categorias_empresa");
            entity.Property(e => e.PuedeGestionarProductosBase)
                .HasDefaultValueSql("'0'")
                .HasColumnName("puede_gestionar_productos_base");
            entity.Property(e => e.PuedeGestionarProductosEmpresa)
                .HasDefaultValueSql("'0'")
                .HasColumnName("puede_gestionar_productos_empresa");
            entity.Property(e => e.PuedeGestionarUsuarios)
                .HasDefaultValueSql("'0'")
                .HasColumnName("puede_gestionar_usuarios");
            entity.Property(e => e.PuedeVerEstadisticas)
                .HasDefaultValueSql("'1'")
                .HasColumnName("puede_ver_estadisticas");
            entity.Property(e => e.Rol)
                .HasDefaultValueSql("'editor'")
                .HasColumnType("enum('admin','editor','viewer')")
                .HasColumnName("rol");
            entity.Property(e => e.UltimoLogin)
                .HasColumnType("timestamp")
                .HasColumnName("ultimo_login");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Empresa).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("usuarios_ibfk_1");
        });

        modelBuilder.Entity<VistaCatalogoEmpresa>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vista_catalogo_empresa");

            entity.Property(e => e.Codigo).HasColumnName("codigo");
            entity.Property(e => e.CodigoBarras)
                .HasMaxLength(100)
                .HasColumnName("codigo_barras");
            entity.Property(e => e.CodigoRubro).HasColumnName("codigo_rubro");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .HasDefaultValueSql("''")
                .HasColumnName("descripcion");
            entity.Property(e => e.DescripcionCorta)
                .HasMaxLength(200)
                .HasColumnName("descripcion_corta");
            entity.Property(e => e.DescripcionLarga)
                .HasColumnType("mediumtext")
                .HasColumnName("descripcion_larga");
            entity.Property(e => e.Destacado).HasColumnName("destacado");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.EmpresaNombre)
                .HasMaxLength(200)
                .HasDefaultValueSql("''")
                .HasColumnName("empresa_nombre");
            entity.Property(e => e.Existencia)
                .HasPrecision(8)
                .HasColumnName("existencia");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ImagenAlt)
                .HasMaxLength(255)
                .HasColumnName("imagen_alt");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(500)
                .HasColumnName("imagen_url");
            entity.Property(e => e.Marca)
                .HasMaxLength(100)
                .HasColumnName("marca");
            entity.Property(e => e.OrdenCategoria).HasColumnName("orden_categoria");
            entity.Property(e => e.Precio)
                .HasPrecision(10, 3)
                .HasColumnName("precio");
            entity.Property(e => e.Tags)
                .HasMaxLength(500)
                .HasColumnName("tags");
            entity.Property(e => e.TipoProducto)
                .HasMaxLength(7)
                .HasDefaultValueSql("''")
                .HasColumnName("tipo_producto");
            entity.Property(e => e.UnidadMedida)
                .HasMaxLength(20)
                .HasColumnName("unidad_medida");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.Visible).HasColumnName("visible");
        });

        modelBuilder.Entity<VistaCategoriasEmpresa>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vista_categorias_empresa");

            entity.Property(e => e.CodigoRubro).HasColumnName("codigo_rubro");
            entity.Property(e => e.Color)
                .HasMaxLength(7)
                .HasColumnName("color");
            entity.Property(e => e.Descripcion)
                .HasColumnType("mediumtext")
                .HasColumnName("descripcion");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.EmpresaNombre)
                .HasMaxLength(200)
                .HasDefaultValueSql("''")
                .HasColumnName("empresa_nombre");
            entity.Property(e => e.Icono)
                .HasMaxLength(50)
                .HasColumnName("icono");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasDefaultValueSql("''")
                .HasColumnName("nombre");
            entity.Property(e => e.Orden).HasColumnName("orden");
            entity.Property(e => e.TipoCategoria)
                .HasMaxLength(7)
                .HasDefaultValueSql("''")
                .HasColumnName("tipo_categoria");
            entity.Property(e => e.Visible).HasColumnName("visible");
        });

        modelBuilder.Entity<ListasPrecio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("listas_precios");

            entity.HasIndex(e => e.Codigo, "uk_codigo").IsUnique();

            entity.HasIndex(e => new { e.Activa, e.Orden }, "idx_activa_orden");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activa)
                .HasDefaultValueSql("'1'")
                .HasColumnType("tinyint(1)")
                .HasColumnName("activa")
                .HasSentinel(false);
            entity.Property(e => e.Codigo)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("codigo");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.EsPredeterminada)
                .HasDefaultValueSql("'0'")
                .HasColumnType("tinyint(1)")
                .HasColumnName("es_predeterminada")
                .HasSentinel(true);
            entity.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Orden)
                .HasDefaultValueSql("'0'")
                .HasColumnName("orden");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<ProductosBasePrecio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("productos_base_precios");

            entity.HasIndex(e => new { e.ProductoBaseId, e.ListaPrecioId }, "uk_producto_lista").IsUnique();

            entity.HasIndex(e => e.ListaPrecioId, "idx_lista_precio");

            entity.HasIndex(e => e.ActualizadoGecom, "idx_actualizado_gecom");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActualizadoGecom)
                .HasColumnType("timestamp")
                .HasColumnName("actualizado_gecom");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.ListaPrecioId).HasColumnName("lista_precio_id");
            // entity.Property(e => e.Precio) // ELIMINADO: campo precio no existe más
            //     .HasPrecision(10, 3)
            //     .HasDefaultValueSql("'0.000'")
            //     .HasColumnName("precio");
            entity.Property(e => e.ProductoBaseId).HasColumnName("producto_base_id");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ListaPrecio).WithMany(p => p.ProductosBasePrecios)
                .HasForeignKey(d => d.ListaPrecioId)
                .HasConstraintName("productos_base_precios_ibfk_2");

            entity.HasOne(d => d.ProductoBase).WithMany(p => p.ProductosBasePrecios)
                .HasForeignKey(d => d.ProductoBaseId)
                .HasConstraintName("productos_base_precios_ibfk_1");
        });

        modelBuilder.Entity<ProductosEmpresaPrecio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("productos_empresa_precios");

            entity.HasIndex(e => new { e.EmpresaId, e.TipoProducto, e.ProductoId, e.ListaPrecioId }, "uk_empresa_producto_lista").IsUnique();

            entity.HasIndex(e => new { e.EmpresaId, e.ListaPrecioId }, "idx_empresa_lista");

            entity.HasIndex(e => new { e.TipoProducto, e.ProductoId }, "idx_tipo_producto");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.ListaPrecioId).HasColumnName("lista_precio_id");
            entity.Property(e => e.PrecioOverride)
                .HasPrecision(10, 3)
                .HasColumnName("precio_override");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.TipoProducto)
                .IsRequired()
                .HasColumnType("enum('base','empresa')")
                .HasColumnName("tipo_producto");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Empresa).WithMany(p => p.ProductosEmpresaPrecios)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("productos_empresa_precios_ibfk_1");

            entity.HasOne(d => d.ListaPrecio).WithMany(p => p.ProductosEmpresaPrecios)
                .HasForeignKey(d => d.ListaPrecioId)
                .HasConstraintName("productos_empresa_precios_ibfk_2");
        });

        modelBuilder.Entity<VistaProductosPreciosEmpresa>(entity =>
        {
            entity.HasNoKey();

            entity.ToView("vista_productos_precios_empresa");

            entity.Property(e => e.ActualizadoGecom)
                .HasColumnType("timestamp")
                .HasColumnName("actualizado_gecom");
            entity.Property(e => e.Codigo).HasColumnName("codigo");
            entity.Property(e => e.CodigoRubro).HasColumnName("codigo_rubro");
            entity.Property(e => e.Descripcion)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("descripcion");
            entity.Property(e => e.Destacado)
                .HasColumnType("tinyint(4)")
                .HasColumnName("destacado");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.EmpresaNombre)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("empresa_nombre");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(500)
                .HasColumnName("imagen_url");
            entity.Property(e => e.ListaCodigo)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("lista_codigo");
            entity.Property(e => e.ListaNombre)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("lista_nombre");
            entity.Property(e => e.ListaPrecioId).HasColumnName("lista_precio_id");
            entity.Property(e => e.Marca)
                .HasMaxLength(100)
                .HasColumnName("marca");
            entity.Property(e => e.PrecioFinal)
                .HasPrecision(10, 3)
                .HasColumnName("precio_final");
            entity.Property(e => e.PrecioPersonalizado)
                .HasColumnType("tinyint(1)")
                .HasColumnName("precio_personalizado");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.TipoProducto)
                .IsRequired()
                .HasMaxLength(7)
                .HasColumnName("tipo_producto");
            entity.Property(e => e.UnidadMedida)
                .HasMaxLength(20)
                .HasColumnName("unidad_medida");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.Visible)
                .HasColumnType("tinyint(4)")
                .HasColumnName("visible");
        });

        modelBuilder.Entity<Agrupaciones>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("agrupaciones");

            entity.HasIndex(e => new { e.Codigo, e.EmpresaPrincipalId }, "idx_agrupaciones_codigo_empresa").IsUnique();

            entity.HasIndex(e => e.EmpresaPrincipalId, "idx_agrupaciones_empresa_principal");

            entity.HasIndex(e => e.Tipo, "idx_agrupaciones_tipo");

            entity.HasIndex(e => new { e.Tipo, e.EmpresaPrincipalId }, "idx_agrupaciones_tipo_empresa");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Codigo).HasColumnName("codigo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .HasColumnName("nombre");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .HasColumnName("descripcion");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.EmpresaPrincipalId).HasColumnName("empresa_principal_id");
            entity.Property(e => e.Tipo)
                .HasDefaultValue(3)
                .HasColumnName("tipo");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.EmpresaPrincipal).WithMany(p => p.Agrupaciones)
                .HasForeignKey(d => d.EmpresaPrincipalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_agrupaciones_empresa_principal");
        });

        modelBuilder.Entity<EmpresasAgrupacionesVisible>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("empresas_agrupaciones_visibles");

            entity.HasIndex(e => new { e.EmpresaId, e.AgrupacionId }, "idx_empresa_agrupacion").IsUnique();

            entity.HasIndex(e => e.AgrupacionId, "idx_agrupacion");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.AgrupacionId).HasColumnName("agrupacion_id");
            entity.Property(e => e.Visible)
                .HasDefaultValue(true)
                .HasColumnName("visible");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Empresa).WithMany(p => p.EmpresasAgrupacionesVisibles)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_empresas_agrupaciones_empresa");

            entity.HasOne(d => d.Agrupacion).WithMany(p => p.EmpresasAgrupacionesVisibles)
                .HasForeignKey(d => d.AgrupacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_empresas_agrupaciones_agrupacion");
        });

        modelBuilder.Entity<EmpresasNovedad>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("empresas_novedades");

            entity.HasIndex(e => new { e.EmpresaId, e.AgrupacionId }, "uk_empresas_novedades_empresa_agrupacion").IsUnique();

            entity.HasIndex(e => e.EmpresaId, "idx_empresas_novedades_empresa");

            entity.HasIndex(e => e.AgrupacionId, "idx_empresas_novedades_agrupacion");

            entity.HasIndex(e => e.Visible, "idx_empresas_novedades_visible");

            entity.HasIndex(e => new { e.EmpresaId, e.Visible }, "idx_empresas_novedades_empresa_visible");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.AgrupacionId).HasColumnName("agrupacion_id");
            entity.Property(e => e.Visible)
                .HasDefaultValue(true)
                .HasColumnName("visible");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Empresa).WithMany(p => p.EmpresasNovedades)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_empresas_novedades_empresa");

            entity.HasOne(d => d.Agrupacion).WithMany(p => p.EmpresasNovedades)
                .HasForeignKey(d => d.AgrupacionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_empresas_novedades_agrupacion");
        });

        modelBuilder.Entity<EmpresasOferta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("empresas_ofertas");

            entity.HasIndex(e => new { e.EmpresaId, e.AgrupacionId }, "uk_empresas_ofertas_empresa_agrupacion").IsUnique();

            entity.HasIndex(e => e.EmpresaId, "idx_empresas_ofertas_empresa");

            entity.HasIndex(e => e.AgrupacionId, "idx_empresas_ofertas_agrupacion");

            entity.HasIndex(e => e.Visible, "idx_empresas_ofertas_visible");

            entity.HasIndex(e => new { e.EmpresaId, e.Visible }, "idx_empresas_ofertas_empresa_visible");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.AgrupacionId).HasColumnName("agrupacion_id");
            entity.Property(e => e.Visible)
                .HasDefaultValue(true)
                .HasColumnName("visible");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Empresa).WithMany(p => p.EmpresasOfertas)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_empresas_ofertas_empresa");

            entity.HasOne(d => d.Agrupacion).WithMany(p => p.EmpresasOfertas)
                .HasForeignKey(d => d.AgrupacionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_empresas_ofertas_agrupacion");
        });

        modelBuilder.Entity<ProductosBaseStock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("productos_base_stock");

            entity.HasIndex(e => new { e.EmpresaId, e.ProductoBaseId }, "uk_empresa_producto").IsUnique();

            entity.HasIndex(e => e.EmpresaId, "idx_empresa");

            entity.HasIndex(e => e.ProductoBaseId, "idx_producto");

            entity.HasIndex(e => e.Existencia, "idx_existencia");

            entity.HasIndex(e => new { e.EmpresaId, e.Existencia }, "idx_empresa_existencia");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.ProductoBaseId).HasColumnName("producto_base_id");
            entity.Property(e => e.Existencia)
                .HasDefaultValue(0.000m)
                .HasPrecision(10, 3)
                .HasColumnName("existencia");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Empresa).WithMany()
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_stock_empresa");

            entity.HasOne(d => d.ProductoBase).WithMany()
                .HasForeignKey(d => d.ProductoBaseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_stock_producto");
        });

        // Feature Flags Configuration
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.FeatureDefinitionConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.EmpresaFeatureConfiguration());
        
        // Users Configuration
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.UserNotificationPreferencesConfiguration());
        
        // Clientes Configuration
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.ClienteConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.ClienteRefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.ClienteLoginHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.CustomerSyncSessionConfiguration());
        
        // Pedidos Configuration
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.PedidoConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.PedidoItemConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.CorreccionTokenConfiguration());
        
        // Delivery Configuration
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.DeliverySettingsConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.DeliveryScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.DeliverySlotConfiguration());

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
