using Microsoft.EntityFrameworkCore;
using Lemmikkitietokanta.Models;

namespace Lemmikkitietokanta.Data
{
    public class LemmikkiDbContext : DbContext
    {
        public LemmikkiDbContext(DbContextOptions<LemmikkiDbContext> options)
            : base(options)
        {
        }

        public DbSet<Lemmikki> Lemmikit { get; set; }
        public DbSet<Omistaja> Omistajat { get; set; }
    }
}