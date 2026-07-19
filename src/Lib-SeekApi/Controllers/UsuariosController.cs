using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lib_SeekApi.Data;
using Lib_SeekApi.Models;

namespace Lib_SeekApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context) { _context = context; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioPerfilDto>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Emprestimos)
                    .ThenInclude(e => e.Livro) 
                .FirstOrDefaultAsync(u => u.Id == id);
                
            if (usuario == null) return NotFound();

            var historicoDto = usuario.Emprestimos.Select(e => new EmprestimoHistoricoDto(
                e.Id,
                e.Livro.Titulo, 
                e.DataEmprestimo,
                e.DataDevolucaoPrevista,
                e.DataDevolucaoReal,
                e.DataDevolucaoReal.HasValue ? "Devolvido" : (e.DataDevolucaoPrevista < DateTime.UtcNow ? "Atrasado" : "Pendente")
            )).ToList();

            var perfilDto = new UsuarioPerfilDto(
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                usuario.Telefone,
                usuario.Ativo,
                historicoDto
            );

            return perfilDto; 
        }

        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario([FromBody] CriarUsuarioDto dto)
        {
            try
            {
                var novoUsuario = new Usuario(dto.Nome, dto.Email, dto.Telefone);
                _context.Usuarios.Add(novoUsuario);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetUsuario), new { id = novoUsuario.Id }, novoUsuario);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); } 
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, [FromBody] AtualizarUsuarioDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound("Usuário não encontrado.");

            try
            {
                usuario.AtualizarDetalhes(dto.Nome, dto.Email, dto.Telefone);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        [HttpPatch("{id}/inativar")]
        public async Task<IActionResult> InativarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            usuario.Inativar();
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/ativar")]
        public async Task<IActionResult> AtivarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound("Usuário não encontrado.");

            usuario.Ativar();
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public record CriarUsuarioDto(string Nome, string Email, string Telefone);
    public record AtualizarUsuarioDto(string Nome, string Email, string Telefone);
    public record UsuarioPerfilDto(int Id, string Nome, string Email, string Telefone, bool Ativo, List<EmprestimoHistoricoDto> Historico);
    public record EmprestimoHistoricoDto(int EmprestimoId, string LivroTitulo, DateTime DataEmprestimo, DateTime DataDevolucaoPrevista, DateTime? DataDevolucaoReal, string Status);
}