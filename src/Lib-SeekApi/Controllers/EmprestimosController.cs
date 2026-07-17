// Controllers/EmprestimosController.cs
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

        public EmprestimosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Emprestimo>>> GetEmprestimos()
        {
            return await _context.Emprestimos
                .Include(e => e.Livro)
                .Include(e => e.Usuario)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Emprestimo>> GetEmprestimo(int id)
        {
            var emprestimo = await _context.Emprestimos
                .Include(e => e.Livro)
                .Include(e => e.Usuario)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emprestimo == null) return NotFound();
            return emprestimo;
        }

        //Registrar novo empréstimo
        [HttpPost]
        public async Task<ActionResult<Emprestimo>> PostEmprestimo([FromBody] Emprestimo emprestimoInput)
        {
            var livro = await _context.Livros.FindAsync(emprestimoInput.LivroId);
            if (livro == null)
                return NotFound("Livro não encontrado.");

            var usuario = await _context.Usuarios.FindAsync(emprestimoInput.UsuarioId);
            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            if (!usuario.Ativo)
                return BadRequest("Usuário inativo não pode realizar empréstimos.");

            try
            {
                livro.RegistrarSaida();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            _context.Emprestimos.Add(emprestimoInput);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmprestimo), new { id = emprestimoInput.Id }, emprestimoInput);
        }

        //Registrar devolução
        [HttpPatch("{id}/devolver")]
        public async Task<IActionResult> DevolverLivro(int id)
        {
            var emprestimo = await _context.Emprestimos.FindAsync(id);
            if (emprestimo == null) return NotFound();

            if (emprestimo.DataDevolucaoReal != null)
                return BadRequest("Este empréstimo já foi devolvido.");

            emprestimo.FinalizarEmprestimo(DateTime.UtcNow);

            var livro = await _context.Livros.FindAsync(emprestimo.LivroId);
            livro?.RegistrarRetorno();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("atrasados")]
        public async Task<ActionResult<IEnumerable<object>>> GetAtrasados()
        {
            var agora = DateTime.UtcNow;

            var emprestimos = await _context.Emprestimos
                .Include(e => e.Livro)
                .Include(e => e.Usuario)
                .Where(e => e.DataDevolucaoReal == null)
                .ToListAsync();

            var atrasados = emprestimos
                .Where(e => e.EstaAtrasado(agora))
                .Select(e => new
                {
                    e.Id,
                    e.LivroId,
                    Livro = e.Livro?.Titulo,
                    e.UsuarioId,
                    Usuario = e.Usuario?.Nome,
                    e.DataEmprestimo,
                    e.DataDevolucaoPrevista,
                    DiasAtraso = e.CalcularDiasAtraso(agora)
                });

            return Ok(atrasados);
        }
    }
}