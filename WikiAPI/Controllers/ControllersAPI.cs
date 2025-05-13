using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WikiAPI.Data;
using WikiAPI.Models;

namespace WikiAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllersAPI : ControllerBase
    {
        private ContextoWiki _contexto;

        public void ArtigoController(ContextoWiki contexto)
        {
            _contexto = contexto;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Artigo>>> ObterArtigos()
        {
            return await _contexto.Artigo.ToListAsync();

        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Artigo>> ObterArtigo(int id)
        {
            var artigo = await _contexto.Artigo.FindAsync(id);
            if (artigo == null)
            {
                return NotFound();
            }
            return artigo;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarArtigo(int id, Artigo artigo)
        {
            if (id != artigo.Id){
                return BadRequest();
            }
            artigo.DataAtualizacao = DateTime.UtcNow;
            _contexto.Entry(artigo).State = EntityState.Modified;
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
        public async Task<ActionResult<Artigo>> CriarArtigo(Artigo artigo)
        {
            artigo.DataCriacao =  DateTime.UtcNow;
            _contexto.Artigo.Add(artigo);
            await _contexto.SaveChangesAsync();

            return CreatedAtAction ("ObterArtigo", new { id = artigo.Id}, artigo);
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
        private bool ArtigoExiste(int id){
            return _contexto.Artigo.Any( e => e.Id == id);
        }
    }
}