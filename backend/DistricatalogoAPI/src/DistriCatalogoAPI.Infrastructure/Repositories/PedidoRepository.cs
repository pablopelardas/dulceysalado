using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Infrastructure.Models;

namespace DistriCatalogoAPI.Infrastructure.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly DistricatalogoContext _context;

        public PedidoRepository(DistricatalogoContext context)
        {
            _context = context;
        }

        public async Task<Pedido?> GetByIdAsync(int id, bool includeItems = true)
        {
            var query = _context.Pedidos.AsQueryable();
            
            if (includeItems)
                query = query.Include(p => p.Items).Include(p => p.Cliente);
            
            return await query.FirstOrDefaultAsync(p => p.Id == id);
        }
        
        public async Task<Pedido?> GetByIdWithItemsAsync(int id)
        {
            return await _context.Pedidos
                .Include(p => p.Items)
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Pedido?> GetByNumeroAsync(string numero, bool includeItems = true)
        {
            var query = _context.Pedidos.AsQueryable();
            
            if (includeItems)
                query = query.Include(p => p.Items).Include(p => p.Cliente);
            
            return await query.FirstOrDefaultAsync(p => p.Numero == numero);
        }

        public async Task<IEnumerable<Pedido>> GetAllAsync(int empresaId, bool includeItems = false)
        {
            var query = _context.Pedidos.Where(p => p.EmpresaId == empresaId);
            
            if (includeItems)
                query = query.Include(p => p.Items);
            
            return await query.Include(p => p.Cliente).ToListAsync();
        }

        public async Task<Pedido> AddAsync(Pedido pedido)
        {
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
            return pedido;
        }

        public async Task UpdateAsync(Pedido pedido)
        {
            _context.Pedidos.Update(pedido);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Pedido pedido)
        {
            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Pedido>> GetByClienteIdAsync(int clienteId, int empresaId, bool includeItems = false)
        {
            var query = _context.Pedidos
                .Where(p => p.ClienteId == clienteId && p.EmpresaId == empresaId);
            
            if (includeItems)
                query = query.Include(p => p.Items);
            
            return await query
                .Include(p => p.Cliente)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Pedido> Items, int TotalCount)> GetPaginatedByClienteAsync(
            int clienteId,
            int empresaId,
            int page = 1,
            int pageSize = 20,
            PedidoEstado? estado = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null)
        {
            var query = _context.Pedidos
                .Where(p => p.ClienteId == clienteId && p.EmpresaId == empresaId);

            if (estado.HasValue)
                query = query.Where(p => p.Estado == estado.Value);

            if (fechaDesde.HasValue)
                query = query.Where(p => p.FechaPedido >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(p => p.FechaPedido <= fechaHasta.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .Include(p => p.Cliente)
                .Include(p => p.Items)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(IEnumerable<Pedido> Items, int TotalCount)> GetPaginatedAsync(
            int empresaId,
            int page = 1,
            int pageSize = 20,
            PedidoEstado? estado = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? clienteId = null,
            string? numeroContiene = null)
        {
            var query = _context.Pedidos.Where(p => p.EmpresaId == empresaId);

            if (estado.HasValue)
                query = query.Where(p => p.Estado == estado.Value);

            if (fechaDesde.HasValue)
                query = query.Where(p => p.FechaPedido >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(p => p.FechaPedido <= fechaHasta.Value);

            if (clienteId.HasValue)
                query = query.Where(p => p.ClienteId == clienteId.Value);

            if (!string.IsNullOrWhiteSpace(numeroContiene))
                query = query.Where(p => p.Numero.Contains(numeroContiene));

            var totalCount = await query.CountAsync();

            var items = await query
                .Include(p => p.Cliente)
                .Include(p => p.Items)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> GetCountByEstadoAsync(int empresaId, PedidoEstado estado)
        {
            return await _context.Pedidos
                .CountAsync(p => p.EmpresaId == empresaId && p.Estado == estado);
        }

        public async Task<decimal> GetMontoTotalByEstadoAsync(int empresaId, PedidoEstado estado, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var query = _context.Pedidos
                .Where(p => p.EmpresaId == empresaId && p.Estado == estado);

            if (fechaDesde.HasValue)
                query = query.Where(p => p.FechaPedido >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(p => p.FechaPedido <= fechaHasta.Value);

            return await query.SumAsync(p => p.MontoTotal);
        }

        public async Task<IEnumerable<Pedido>> GetPendientesAsync(int empresaId)
        {
            return await _context.Pedidos
                .Where(p => p.EmpresaId == empresaId && p.Estado == PedidoEstado.Pendiente)
                .Include(p => p.Cliente)
                .Include(p => p.Items)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pedido>> GetRecentesAsync(int empresaId, int dias = 7)
        {
            var fechaLimite = DateTime.UtcNow.AddDays(-dias);
            
            return await _context.Pedidos
                .Where(p => p.EmpresaId == empresaId && p.CreatedAt >= fechaLimite)
                .Include(p => p.Cliente)
                .OrderByDescending(p => p.CreatedAt)
                .Take(50)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Pedidos.AnyAsync(p => p.Id == id);
        }

        public async Task<bool> ExisteNumeroAsync(string numero)
        {
            return await _context.Pedidos.AnyAsync(p => p.Numero == numero);
        }

        public async Task<string> GenerateUniqueNumeroAsync()
        {
            string numero;
            int attempts = 0;
            const int maxAttempts = 10;

            do
            {
                var fecha = DateTime.UtcNow.ToString("yyyyMMdd");
                var random = new Random().Next(1000, 9999);
                numero = $"PED-{fecha}-{random}";
                
                var exists = await ExisteNumeroAsync(numero);
                if (!exists) return numero;
                
                attempts++;
            } while (attempts < maxAttempts);

            throw new InvalidOperationException("No se pudo generar un número único para el pedido");
        }

        public async Task<bool> CambiarEstadoAsync(int pedidoId, PedidoEstado nuevoEstado, int usuarioId, string? motivo = null)
        {
            var pedido = await GetByIdAsync(pedidoId, false);
            if (pedido == null) return false;

            try
            {
                switch (nuevoEstado)
                {
                    case PedidoEstado.Aceptado:
                        pedido.AceptarPedido(usuarioId);
                        break;
                    case PedidoEstado.Rechazado:
                        pedido.RechazarPedido(usuarioId, motivo ?? "Sin especificar");
                        break;
                    case PedidoEstado.Completado:
                        pedido.CompletarPedido(usuarioId);
                        break;
                    case PedidoEstado.Cancelado:
                        pedido.CancelarPedido(usuarioId, motivo ?? "Cancelado");
                        break;
                }

                await UpdateAsync(pedido);
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        // Items de pedido
        public async Task<PedidoItem?> GetItemByIdAsync(int itemId)
        {
            return await _context.PedidoItems
                .Include(i => i.Pedido)
                .FirstOrDefaultAsync(i => i.Id == itemId);
        }

        public async Task<IEnumerable<PedidoItem>> GetItemsByPedidoIdAsync(int pedidoId)
        {
            return await _context.PedidoItems
                .Where(i => i.PedidoId == pedidoId)
                .ToListAsync();
        }

        public async Task AddItemAsync(PedidoItem item)
        {
            _context.PedidoItems.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateItemAsync(PedidoItem item)
        {
            _context.PedidoItems.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(PedidoItem item)
        {
            _context.PedidoItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}