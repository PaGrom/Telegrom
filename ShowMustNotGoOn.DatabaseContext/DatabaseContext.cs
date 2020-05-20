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
        public DbSet<MessageText> MessageTexts { get; set; }
        public DbSet<BotMessage> BotMessages { get; set; }
        public DbSet<Callback> Callbacks { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<StateAttribute> StateAttributes { get; set; }

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

            modelBuilder.Entity<MessageText>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.RowVersion)
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<BotMessage>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne<IdentityUser>()
                    .WithMany()
                    .HasForeignKey(c => c.UserId);

                entity.HasOne<MessageText>()
                    .WithMany()
                    .HasForeignKey(c => c.MessageTextId);
            });

            modelBuilder.Entity<Callback>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne<BotMessage>()
                    .WithMany()
                    .HasForeignKey(c => c.BotMessageId);

                entity.Property(e => e.RowVersion)
                    .IsConcurrencyToken();
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

            modelBuilder.Entity<StateAttribute>(entity =>
            {
                entity.HasKey(e => new { e.Id });

                entity.HasOne<IdentityUser>()
                    .WithMany()
                    .HasForeignKey(c => c.UserId);
            });
        }
    }
}
