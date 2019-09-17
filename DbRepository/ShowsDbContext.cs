using DbRepository.Model;
using Microsoft.EntityFrameworkCore;

namespace DbRepository
{
    public class ShowsDbContext : DbContext
    {
        public ShowsDbContext(DbContextOptions options) : base(options) { }

        public DbSet<TvShow> TvShows { get; set; }
    }
}
