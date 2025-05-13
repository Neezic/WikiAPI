using Microsoft.EntityFrameworkCore;

namespace WikiAPI
{
    public class ContextoWiki : DbContext
    {
        public ContextoWiki(DbContextOptions<ContextoWiki> options) : base(options) { }

        public DbSet<Artigo> TabelaArtigo => Set<Artigo>();

    }


}