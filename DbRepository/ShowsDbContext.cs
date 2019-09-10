using Microsoft.EntityFrameworkCore;

namespace DbRepository
{
    public class ShowsDbContext : DbContext
    {
        public DbSet<TvShow> TvShows { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=tvshows.db");
        }
    }

    public class TvShow
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string TitleOriginal { get; set; }
        public string Description { get; set; }
        public long TotalSeasons { get; set; }
        public string Status { get; set; }
    }
}
