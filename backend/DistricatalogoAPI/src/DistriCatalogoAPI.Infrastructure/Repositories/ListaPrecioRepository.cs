using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Infrastructure.Models;

namespace DistriCatalogoAPI.Infrastructure.Repositories
{
    public class ListaPrecioRepository : IListaPrecioRepository
    {
        private readonly DistricatalogoContext _context;
        private readonly ILogger<ListaPrecioRepository> _logger;

        public ListaPrecioRepository(
            DistricatalogoContext context,
            ILogger<ListaPrecioRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int?> GetIdByCodigoAsync(string codigo)
        {
            try
            {
                var lista = await _context.ListasPrecios
                    .Where(l => l.Codigo == codigo && l.Activa)
                    .FirstOrDefaultAsync();
                    
                return lista?.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ID de lista de precios por código {Codigo}", codigo);
                throw;
            }
        }

        public async Task<bool> ExistsAndActiveAsync(string codigo)
        {
            try
            {
                return await _context.ListasPrecios
                    .AnyAsync(l => l.Codigo == codigo && l.Activa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de lista de precios activa {Codigo}", codigo);
                throw;
            }
        }

        public async Task<(string codigo, string nombre)?> GetCodigoAndNombreByIdAsync(int id)
        {
            try
            {
                var lista = await _context.ListasPrecios
                    .Where(l => l.Id == id)
                    .Select(l => new { l.Codigo, l.Nombre })
                    .FirstOrDefaultAsync();
                    
                return lista != null ? (lista.Codigo ?? "", lista.Nombre ?? "") : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener código y nombre de lista de precios {Id}", id);
                throw;
            }
        }

        public async Task<List<ListaPrecioDto>> GetAllActiveAsync()
        {
            try
            {
                var listas = await _context.ListasPrecios
                    .Where(l => l.Activa)
                    .OrderBy(l => l.Orden)
                    .ThenBy(l => l.Codigo)
                    .Select(l => new ListaPrecioDto
                    {
                        Id = l.Id,
                        Codigo = l.Codigo ?? "",
                        Nombre = l.Nombre ?? "",
                        EsPredeterminada = l.EsPredeterminada,
                        Activa = l.Activa
                    })
                    .ToListAsync();

                return listas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener listas de precios activas");
                throw;
            }
        }

        public async Task<ListaPrecioDto?> GetByIdAsync(int id)
        {
            try
            {
                var lista = await _context.ListasPrecios
                    .Where(l => l.Id == id)
                    .Select(l => new ListaPrecioDto
                    {
                        Id = l.Id,
                        Codigo = l.Codigo ?? "",
                        Nombre = l.Nombre ?? "",
                        EsPredeterminada = l.EsPredeterminada || false,
                        Activa = l.Activa
                    })
                    .FirstOrDefaultAsync();

                return lista;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lista de precios {Id}", id);
                throw;
            }
        }

        public async Task<bool> TienePreciosAsociadosAsync(int id)
        {
            try
            {
                // Verificar si tiene precios en productos base
                var tieneBasePrecio = await _context.ProductosBasePrecios
                    .AnyAsync(p => p.ListaPrecioId == id);

                if (tieneBasePrecio)
                    return true;

                // Verificar si tiene precios en productos empresa
                var tieneEmpresaPrecio = await _context.ProductosEmpresaPrecios
                    .AnyAsync(p => p.ListaPrecioId == id);

                return tieneEmpresaPrecio;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar precios asociados para lista {Id}", id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var lista = await _context.ListasPrecios
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (lista != null)
                {
                    _context.ListasPrecios.Remove(lista);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Lista de precios {Id} eliminada exitosamente", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar lista de precios {Id}", id);
                throw;
            }
        }

        public async Task<int> CreateAsync(string codigo, string nombre, string? descripcion, int? orden)
        {
            try
            {
                var lista = new ListasPrecio
                {
                    Codigo = codigo,
                    Nombre = nombre,
                    Descripcion = descripcion,
                    Orden = orden ?? 0,
                    Activa = true,
                    EsPredeterminada = false
                };

                _context.ListasPrecios.Add(lista);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Lista de precios {Codigo} creada exitosamente con ID {Id}", codigo, lista.Id);
                return lista.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear lista de precios {Codigo}", codigo);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(int id, string? codigo, string? nombre, string? descripcion, bool? activa, int? orden)
        {
            try
            {
                var lista = await _context.ListasPrecios
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (lista == null)
                    return false;

                if (!string.IsNullOrEmpty(codigo))
                    lista.Codigo = codigo;

                if (!string.IsNullOrEmpty(nombre))
                    lista.Nombre = nombre;
                
                if (descripcion != null)
                    lista.Descripcion = descripcion;
                
                if (activa.HasValue)
                    lista.Activa = activa.Value;
                
                if (orden.HasValue)
                    lista.Orden = orden.Value;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Lista de precios {Id} actualizada exitosamente", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar lista de precios {Id}", id);
                throw;
            }
        }

        public async Task<bool> CodigoExistsAsync(string codigo)
        {
            try
            {
                return await _context.ListasPrecios
                    .AnyAsync(l => l.Codigo == codigo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de código {Codigo}", codigo);
                throw;
            }
        }

        public async Task<bool> IsDefaultListAsync(int id)
        {
            try
            {
                return await _context.ListasPrecios
                    .Where(l => l.Id == id)
                    .Select(l => l.EsPredeterminada)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si la lista {Id} es predeterminada", id);
                throw;
            }
        }

        public async Task<int?> GetDefaultListIdAsync()
        {
            try
            {
                var defaultList = await _context.ListasPrecios
                    .Where(l => l.Activa && l.EsPredeterminada)
                    .Select(l => l.Id)
                    .FirstOrDefaultAsync();
                
                return defaultList == 0 ? null : defaultList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lista de precios predeterminada");
                throw;
            }
        }

        public async Task SetDefaultAsync(int id)
        {
            try
            {
                // Primero desmarcar todas las listas como predeterminadas
                await _context.ListasPrecios
                    .Where(l => l.EsPredeterminada)
                    .ExecuteUpdateAsync(l => l.SetProperty(p => p.EsPredeterminada, false));

                // Marcar la lista especificada como predeterminada
                var rowsAffected = await _context.ListasPrecios
                    .Where(l => l.Id == id && l.Activa)
                    .ExecuteUpdateAsync(l => l.SetProperty(p => p.EsPredeterminada, true));

                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException($"No se pudo marcar la lista {id} como predeterminada. Verifique que exista y esté activa.");
                }

                _logger.LogInformation("Lista de precios ID {Id} marcada como predeterminada", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar lista de precios ID {Id} como predeterminada", id);
                throw;
            }
        }
    }
}