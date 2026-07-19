using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lib_SeekApi.Data;
using Lib_SeekApi.Models;

namespace Lib_SeekApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmprestimosController : ControllerBase
    {
        private readonly AppDbContext _context;
        public EmprestimosController(AppDbContext context) { _context = context; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmprestimoResponseDto>>> GetEmprestimos()
        {
            var emprestimos = await _context.Emprestimos
                .Include(e => e.Livro)
                .Include(e => e.Usuario)
                .ToListAsync();

            return Ok(emprestimos.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmprestimoResponseDto>> GetEmprestimo(int id)
        {
            var emprestimo = await _context.Emprestimos
                .Include(e => e.Livro)
                .Include(e => e.Usuario)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emprestimo == null) return NotFound();

            return Ok(MapToDto(emprestimo));
        }

        [HttpPost]
        public async Task<ActionResult<EmprestimoResponseDto>> PostEmprestimo([FromBody] CriarEmprestimoDto dto)
        {
            try
            {
                var livro = await _context.Livros.FindAsync(dto.LivroId);
                var usuario = await _context.Usuarios.FindAsync(dto.UsuarioId);

                if (livro == null || usuario == null) return NotFound("Livro ou Usuário não encontrado.");
                if (livro.QuantidadeEstoque <= 0) return BadRequest("Livro sem estoque.");

                var novoEmprestimo = new Emprestimo(dto.LivroId, dto.UsuarioId);
                
                _context.Emprestimos.Add(novoEmprestimo);
                livro.RegistrarSaida();
                
                await _context.SaveChangesAsync();
                
                await _context.Entry(novoEmprestimo).Reference(e => e.Livro).LoadAsync();
                await _context.Entry(novoEmprestimo).Reference(e => e.Usuario).LoadAsync();

                return CreatedAtAction("GetEmprestimo", new { id = novoEmprestimo.Id }, MapToDto(novoEmprestimo));
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPatch("{id}/devolver")]
        public async Task<IActionResult> Devolver(int id)
        {
            var emprestimo = await _context.Emprestimos.Include(e => e.Livro).FirstOrDefaultAsync(e => e.Id == id);
            if (emprestimo == null) return NotFound();

            emprestimo.FinalizarEmprestimo(DateTime.UtcNow);
            emprestimo.Livro.RegistrarRetorno();
            
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/renovar")]
        public async Task<IActionResult> Renovar(int id, [FromBody] RenovarEmprestimoDto dto)
        {
            var emprestimo = await _context.Emprestimos.FindAsync(id);
            if (emprestimo == null) return NotFound();

            try
            {
                emprestimo.RenovarEmprestimo(dto.DiasExtras);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message); 
            }
        }

        private static EmprestimoResponseDto MapToDto(Emprestimo e) => new EmprestimoResponseDto(
            e.Id,
            new LivroResumidoDto(e.Livro.Id, e.Livro.Titulo),
            new UsuarioResumidoDto(e.Usuario.Id, e.Usuario.Nome),
            e.DataEmprestimo,
            e.DataDevolucaoPrevista,
            e.DataDevolucaoReal
        );
    }
    
    public record CriarEmprestimoDto(int LivroId, int UsuarioId);
    public record RenovarEmprestimoDto(int DiasExtras = 7); 
    public record EmprestimoResponseDto(int Id, LivroResumidoDto Livro, UsuarioResumidoDto Usuario, DateTime DataEmprestimo, DateTime DataDevolucaoPrevista, DateTime? DataDevolucaoReal);
    public record LivroResumidoDto(int Id, string Titulo);
    public record UsuarioResumidoDto(int Id, string Nome);
}