using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lib_SeekApi.Data;
using Lib_SeekApi.Models;
using Lib_SeekApi.Enums;

namespace Lib_SeekApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MultasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MultasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MultaResponseDto>>> GetMultas()
        {
            var multas = await _context.Multas
                .Include(m => m.Emprestimo)
                    .ThenInclude(e => e.Usuario)
                .Include(m => m.Emprestimo)
                    .ThenInclude(e => e.Livro)
                .ToListAsync();

            return Ok(multas.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MultaResponseDto>> GetMulta(int id)
        {
            var multa = await _context.Multas
                .Include(m => m.Emprestimo)
                    .ThenInclude(e => e.Usuario)
                .Include(m => m.Emprestimo)
                    .ThenInclude(e => e.Livro)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (multa == null) return NotFound();
            
            return Ok(MapToDto(multa));
        }

        [HttpGet("pendentes")]
        public async Task<ActionResult<IEnumerable<MultaResponseDto>>> GetMultasPendentes()
        {
            var multas = await _context.Multas
                .Include(m => m.Emprestimo)
                    .ThenInclude(e => e.Usuario)
                .Include(m => m.Emprestimo)
                    .ThenInclude(e => e.Livro)
                .Where(m => m.Status == StatusMulta.Pendente)
                .ToListAsync();

            return Ok(multas.Select(MapToDto));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutMulta(int id, [FromBody] UpdateMultaDto dto)
        {
            var multa = await _context.Multas.FindAsync(id);
            if (multa == null) return NotFound("Multa não encontrada.");

            try
            {
                multa.AtualizarValor(dto.Valor);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/pagar")]
        public async Task<IActionResult> PagarMulta(int id)
        {
            var multa = await _context.Multas.FindAsync(id);
            if (multa == null) return NotFound("Multa não encontrada.");

            try
            {
                multa.Pagar(DateTime.UtcNow);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/cancelar")]
        public async Task<IActionResult> CancelarMulta(int id)
        {
            var multa = await _context.Multas.FindAsync(id);
            if (multa == null) return NotFound("Multa não encontrada.");

            try
            {
                multa.Cancelar();
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private static MultaResponseDto MapToDto(Multa m) => new MultaResponseDto(
            m.Id,
            m.Valor,
            m.Status.ToString(), 
            m.DataGeracao,
            m.DataPagamento,
            new EmprestimoInfoParaMultaDto(
                m.EmprestimoId,
                m.Emprestimo?.Usuario?.Nome ?? "N/A",
                m.Emprestimo?.Livro?.Titulo ?? "N/A"
            )
        );
    }

    public record UpdateMultaDto(decimal Valor);
    public record MultaResponseDto(int Id, decimal Valor, string Status, DateTime DataGeracao, DateTime? DataPagamento, EmprestimoInfoParaMultaDto EmprestimoDetalhes);
    public record EmprestimoInfoParaMultaDto(int EmprestimoId, string NomeUsuario, string TituloLivro);
}