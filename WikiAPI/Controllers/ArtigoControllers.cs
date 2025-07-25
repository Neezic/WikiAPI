using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WikiAPI.Data;
using WikiAPI.Models;

namespace WikiAPI.Controllers
{
    [ApiController]
    [Route("api/artigos")]
    public class ArtigoControllers : ControllerBase
    {
        private readonly ContextoWiki _contexto;

        public ArtigoControllers(ContextoWiki contexto)
        {
            _contexto = contexto;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ArtigoDto>> ObterArtigo(int id)
        {
            var artigo = await _contexto.Artigo
                .Include(a => a.Usuario)
                .Where(a => a.Id == id)
                .Select(a => new ArtigoDto
                {
                    Id = a.Id,
                    Titulo = a.Titulo,
                    Conteudo = a.Conteudo,
                    DataCriacao = a.DataCriacao,
                    DataAtualizacao = a.DataAtualizacao,
                    Autor = a.Usuario != null ? a.Usuario.Nome : "Sistema" // Corrigido aqui
                })
                .FirstOrDefaultAsync();

            return artigo == null ? NotFound() : artigo;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArtigoDto>>> ObterVariosArtigos()
        {
            return await _contexto.Artigo
        .Include(a => a.Usuario)
        .Select(a => new ArtigoDto
        {
            Id = a.Id,
            Titulo = a.Titulo,
            Conteudo = a.Conteudo,
            DataCriacao = a.DataCriacao,
            DataAtualizacao = a.DataAtualizacao,
            Autor = a.Usuario != null ? a.Usuario.Nome : "Sistema"
        })
        .ToListAsync();
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "Editor")]
        public async Task<IActionResult> AtualizarArtigo(int id, Artigo artigo)
        {
            if (id != artigo.Id) return BadRequest();
            
            var artigoExistente = await _contexto.Artigo.FindAsync(id);
            if (artigoExistente == null) return NotFound();
            
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"); // Corrigido aqui
            var usuario = await _contexto.Usuario.FindAsync(usuarioId);

            artigoExistente.Titulo = artigo.Titulo;
            artigoExistente.Conteudo = artigo.Conteudo;
            artigoExistente.DataAtualizacao = DateTime.UtcNow;
            artigoExistente.UsuarioId = usuario?.Id;

            try
            {
                await _contexto.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArtigoExiste(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpPost]
        [Authorize(Policy = "Editor")]
        public async Task<ActionResult<Artigo>> CriarArtigo(Artigo artigo)
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"); // Corrigido aqui
            var usuario = await _contexto.Usuario.FindAsync(usuarioId);

            artigo.DataCriacao = DateTime.UtcNow;
            artigo.UsuarioId = usuario?.Id;

            _contexto.Artigo.Add(artigo);
            await _contexto.SaveChangesAsync();

            return CreatedAtAction(nameof(ObterArtigo), new { id = artigo.Id }, artigo);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "Editor")]
        public async Task<IActionResult> ExcluirArtigo(int id)
        {
            var artigo = await _contexto.Artigo.FindAsync(id);
            if (artigo == null) return NotFound();

            _contexto.Artigo.Remove(artigo);
            await _contexto.SaveChangesAsync();

            return NoContent();
        }

        private bool ArtigoExiste(int id) => _contexto.Artigo.Any(e => e.Id == id);
    }

    public class ArtigoDto
    {
        public int Id { get; set; }
        public required string Titulo { get; set; }
        public required string Conteudo { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public required string Autor { get; set; }
    }
}