using Microsoft.EntityFrameworkCore;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.DatabaseContext
{
    public sealed class DatabaseContext : DbContext
    {
        public DatabaseContext()
        {
        }

        public DatabaseContext(DbContextOptions options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<TvShow> TvShows { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ButtonCallbackQueryData> ButtonCallbackQueryDatas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TvShow>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.HasIndex(e => e.MyShowsId)
                    .IsUnique();

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.HasIndex(e => e.TelegramId)
                    .IsUnique();

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<UserTvShows>(entity =>
            {
                entity.HasKey(nameof(UserTvShows.UserId), nameof(UserTvShows.TvShowId));
            });

            modelBuilder.Entity<UserTvShows>()
                .HasOne(pt => pt.User)
                .WithMany(p => p.TvShows)
                .HasForeignKey(pt => pt.UserId);

            modelBuilder.Entity<UserTvShows>()
                .HasOne(pt => pt.TvShow)
                .WithMany(t => t.Users)
                .HasForeignKey(pt => pt.TvShowId);

            modelBuilder.Entity<ButtonCallbackQueryData>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.MessageId)
                    .IsRequired();

                entity.Property(e => e.Data)
                    .IsRequired();
            });
        }
    }
}
