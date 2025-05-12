using Microsoft.EntityFrameworkCore;

namespace WikiAPI
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Artigo> TabelaProdutos => Set<Artigo>();

    }


}