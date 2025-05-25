using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using WikiAPI.Models;

namespace WikiAPI.Data
{
    public class ContextoWiki : DbContext
    {
        public ContextoWiki(DbContextOptions<ContextoWiki> options) :
        base(options)
        {

        }

        public DbSet<Artigo> Artigo => Set<Artigo>();
        public DbSet<Usuario> UsuarioAplicacao { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Artigo>().ToTable("Artigo");
            modelBuilder.Entity<Usuario>().ToTable("Usuario");
        }

    }



}