using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DbRepository.Entities
{
    public partial class ShowsDbContext : DbContext
    {
        public ShowsDbContext()
        {
        }

        public ShowsDbContext(DbContextOptions<ShowsDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<TvShows> TvShows { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<TvShows>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.HasIndex(e => e.MyShowsId)
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });
        }
    }
}
