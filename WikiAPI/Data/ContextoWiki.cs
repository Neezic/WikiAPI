using Microsoft.EntityFrameworkCore;
using WikiAPI.Models;

namespace WikiAPI.Data
{
    public class ContextoWiki : DbContext
    {
        public ContextoWiki(DbContextOptions<ContextoWiki> options) : base(options) { }

        public DbSet<Artigo> Artigo => Set<Artigo>();

    }


}