using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lib_SeekApi.Data;
using Lib_SeekApi.Models;

namespace Lib_SeekApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LivrosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LivrosController(AppDbContext context) { _context = context; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Livro>>> GetLivros()
        {
            return await _context.Livros.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LivroDetalheDto>> GetLivro(int id)
        {
            var livro = await _context.Livros
                .Include(l => l.Emprestimos)
                    .ThenInclude(e => e.Usuario)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (livro == null) return NotFound();

            var emprestimosAtuais = livro.Emprestimos
                .Where(e => !e.DataDevolucaoReal.HasValue)
                .Select(e => new EmprestimoResumidoDto(
                    e.Id,
                    e.Usuario.Nome,
                    e.DataDevolucaoPrevista,
                    e.DataDevolucaoPrevista < DateTime.UtcNow ? "Atrasado" : "Em curso"
                )).ToList();

            var dto = new LivroDetalheDto(
                livro.Id,
                livro.Titulo,
                livro.QuantidadeEstoque,
                emprestimosAtuais
            );

            return dto;
        }

        [HttpPost]
        public async Task<ActionResult<Livro>> PostLivro([FromBody] Livro livro)
        {
            _context.Livros.Add(livro);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetLivro), new { id = livro.Id }, livro);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutLivro(int id, [FromBody] UpdateLivroDto dto)
        {
            var livro = await _context.Livros.FindAsync(id);
            if (livro == null) return NotFound();

            try
            {
                livro.AtualizarDetalhes(dto.Titulo, dto.Autor, dto.AnoPublicacao);
                
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public record LivroDetalheDto(int Id, string Titulo, int EstoqueAtual, List<EmprestimoResumidoDto> EmprestimosAtuais);
    public record EmprestimoResumidoDto(int EmprestimoId, string NomeLeitor, DateTime DataDevolucaoPrevista, string Status);
    public record UpdateLivroDto(string Titulo, string Autor, int AnoPublicacao);
}