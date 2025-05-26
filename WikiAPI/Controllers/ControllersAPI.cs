using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WikiAPI.Data;
using WikiAPI.Models;

namespace WikiAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ControllersAPI : ControllerBase
    {
        private ContextoWiki _contexto;

        public void ArtigoController(ContextoWiki contexto)
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
                Autor = a.Usuario != null ? a.Usuario.Nome : "Sistema"
            })
            .FirstOrDefaultAsync();
            if (artigo == null)
            {
                return NotFound();
            }
            return artigo;
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "Editor")]
        public async Task<IActionResult> AtualizarArtigo(int id, Artigo artigo)
        {
            if (id != artigo.Id) {
                return BadRequest();
            }
            var artigoExiste = await _contexto.Artigo.FindAsync(id);
            if (artigoExiste == null) return NotFound();
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var usuario = await _contexto.Usuario.FindAsync(usuarioId);

            artigoExiste.Titulo = artigo.Titulo;
            artigoExiste.Conteudo = artigo.Conteudo;
            artigoExiste.DataAtualizacao = DateTime.UtcNow;
            artigoExiste.UsuarioId = usuario?.Id;
            try
            {
                await _contexto.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArtigoExiste(id))
                {
                    return NotFound();

                } else
                {
                    throw;
                }
            }
            return NoContent();

        }
        [HttpPost]
        [Authorize(Policy = "Editor")]
        public async Task<ActionResult<Artigo>> CriarArtigo(Artigo artigo)
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var usuario = await _contexto.Usuario.FindAsync(usuarioId);
            artigo.DataCriacao = DateTime.UtcNow;
            _contexto.Artigo.Add(artigo);
            await _contexto.SaveChangesAsync();

            return CreatedAtAction("ObterArtigo", new { id = artigo.Id }, artigo);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> ExcluirArtigo(int id)
        {
            var artigo = await _contexto.Artigo.FindAsync(id);
            if (artigo == null)
            {
                return NotFound();
            }
            _contexto.Artigo.Remove(artigo);
            await _contexto.SaveChangesAsync();
            return NoContent();
        }
        private bool ArtigoExiste(int id)
        {
            return _contexto.Artigo.Any(e => e.Id == id);
        }


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