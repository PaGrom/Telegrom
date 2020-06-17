using Microsoft.EntityFrameworkCore;
using Telegrom.Database.Model;

namespace Telegrom.Database
{
    public sealed class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<IdentityUser> IdentityUsers { get; set; }
        public DbSet<GlobalAttribute> GlobalAttributes { get; set; }
        public DbSet<SessionAttribute> SessionAttributes { get; set; }
        public DbSet<IdentityState> IdentityStates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityUser>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.RowVersion)
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<GlobalAttribute>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.Type });

                entity.Property(e => e.RowVersion)
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<SessionAttribute>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.SessionId, AttributeType = e.Type });

                entity.Property(e => e.RowVersion)
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<IdentityState>(entity =>
            {
                entity.HasKey(e => e.IdentityId);

                entity.Property(e => e.RowVersion)
                    .IsConcurrencyToken();
            });
        }
    }
}
