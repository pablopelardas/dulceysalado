using Microsoft.EntityFrameworkCore;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Infrastructure.Models;

namespace DistriCatalogoAPI.Infrastructure.Repositories
{
    public class CorreccionTokenRepository : ICorreccionTokenRepository
    {
        private readonly DistricatalogoContext _context;

        public CorreccionTokenRepository(DistricatalogoContext context)
        {
            _context = context;
        }

        public async Task<CorreccionToken?> GetByTokenAsync(string token)
        {
            return await _context.CorrecionTokens
                .Include(ct => ct.Pedido)
                    .ThenInclude(p => p.Cliente)
                .Include(ct => ct.Pedido.Items)
                .FirstOrDefaultAsync(ct => ct.Token == token);
        }

        public async Task<CorreccionToken?> GetByIdAsync(int id)
        {
            return await _context.CorrecionTokens
                .Include(ct => ct.Pedido)
                .FirstOrDefaultAsync(ct => ct.Id == id);
        }

        public async Task<List<CorreccionToken>> GetByPedidoIdAsync(int pedidoId)
        {
            return await _context.CorrecionTokens
                .Where(ct => ct.PedidoId == pedidoId)
                .OrderByDescending(ct => ct.FechaCreacion)
                .ToListAsync();
        }

        public async Task<CorreccionToken> AddAsync(CorreccionToken token)
        {
            _context.CorrecionTokens.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<CorreccionToken> UpdateAsync(CorreccionToken token)
        {
            _context.CorrecionTokens.Update(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task DeleteAsync(int id)
        {
            var token = await GetByIdAsync(id);
            if (token != null)
            {
                _context.CorrecionTokens.Remove(token);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<CorreccionToken>> GetExpiradosAsync()
        {
            var ahora = DateTime.UtcNow;
            return await _context.CorrecionTokens
                .Where(ct => ct.FechaExpiracion < ahora && !ct.Usado)
                .ToListAsync();
        }
    }
}