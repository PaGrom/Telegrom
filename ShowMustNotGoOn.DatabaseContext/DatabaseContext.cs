using Microsoft.EntityFrameworkCore;
using ShowMustNotGoOn.DatabaseContext.Model;

namespace ShowMustNotGoOn.DatabaseContext
{
    public sealed class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<TvShow> TvShows { get; set; }
        public DbSet<IdentityUser> IdentityUsers { get; set; }
        public DbSet<BotMessage> BotMessages { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TvShow>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.RowVersion)
	                .IsConcurrencyToken();

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<IdentityUser>(entity =>
            {
	            entity.HasKey(e => e.Id);

                entity.Property(e => e.RowVersion)
	                .IsConcurrencyToken();
            });

            modelBuilder.Entity<BotMessage>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.HasIndex(e => e.MessageId)
                    .IsUnique();

                entity.HasOne<IdentityUser>()
	                .WithMany()
	                .HasForeignKey(c => c.UserId);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.HasOne<IdentityUser>()
	                .WithMany()
	                .HasForeignKey(c => c.UserId);

                entity.HasOne<TvShow>()
	                .WithMany()
	                .HasForeignKey(c => c.TvShowId);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
            });
        }
    }
}
