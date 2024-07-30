using Microsoft.EntityFrameworkCore;
using SalesOrderApp.Models;

namespace SalesOrderApp.Data
{
    public class SalesOrderAppDbContext : DbContext
    {
        public SalesOrderAppDbContext(DbContextOptions<SalesOrderAppDbContext> options) : base(options)
        {
        }

        #region TABLES

        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        #endregion

        #region METHODS

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed data for UserRoles
            modelBuilder.Entity<UserRole>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasData(
                    Enum.GetValues(typeof(UserRoleEnum))
                        .Cast<UserRoleEnum>()
                        .Select(e => new UserRole
                        {
                            Id = (int)e,
                            Name = e.ToString()
                        })
                );
            });

            // Define any special constraints and relationships

            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Email).IsUnique();
                e.HasOne(x => x.UserRole)
                    .WithMany()
                    .HasForeignKey(x => x.UserRoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

        }

        public override int SaveChanges()
        {
            InterceptSaveChanges();

            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            InterceptSaveChanges();

            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Automatically assigns the values to each entity's DateCreated & DateUpdated fields
        /// </summary>
        private void InterceptSaveChanges()
        {
            var entries = ChangeTracker
            .Entries()
            .Where(e =>
                    e.Entity is BaseEntity && (
                        e.State == EntityState.Added ||
                        e.State == EntityState.Modified
                    )
                );

            foreach (var entityEntry in entries)
            {
                ((BaseEntity)entityEntry.Entity).DateUpdated = DateTime.Now;
                ((BaseEntity)entityEntry.Entity).DateCreated = entityEntry.State == EntityState.Added ? DateTime.Now : ((BaseEntity)entityEntry.Entity).DateCreated;
            }
        }

        #endregion
    }
}
