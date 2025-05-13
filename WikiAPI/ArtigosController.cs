using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WikiAPI
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArtigosController : ControllerBase
    {
        private readonly ContextoWiki _contexto;
        public ArtigosController(ContextoWiki contexto){
            _contexto = contexto;
        }
        //GET: api/artigos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Artigo>> ObterArtigo(int id){
            var artigo = await _contexto.Artigos.FindAsync(id);
            if (artigo == null){
                return NotFound();
            }
            return artigo;

        }
         //GET: api/artigos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarArtigo(int id, Artigo artigo){
            if (id != artigo.Id){
                return BadRequest();
            }
            artigo.DataAtualizacao = DateTime.UtcNow;
            _contexto.Entry(artigo).State = EntityState.Modified;
            try{
                await _contexto.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException){
                if (!ArtigoExiste(id)){
                    return NotFound();
                }
                else {
                    throw;
                }
            }
            return NoContent();
        }
        [HttpPost]
        public async Task<ActionResult<Artigo>> CriarArtigo(Artigo artigo){
            artigo.DataCriacao = DateTime.UtcNow;
            _contexto.Artigos.Add(artigo);
            await _contexto.SaveChangesAsync();
            return CreateAtAction("Obter Artigo", new{id = artigo.Id}, artigo);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult>ExcluirArtigo(int id){
            var artigo = await _contexto.Artigos.FindAsync(id);
            if (artigo == null){
                return Notfound();
            }
            _contexto.Artigos.Remove(artigo);
            await _contexto.SaveChangesAsync();
            return NoContent();

        }
        private bool ArtigoExiste(int id){
            return _contexto.Artigos.Any(e => e.Id == id);
        }
    }
}