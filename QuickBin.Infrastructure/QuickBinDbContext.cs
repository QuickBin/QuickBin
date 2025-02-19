using System.ComponentModel;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using XYZ_Stats.Domain.Entitys;
using XYZ_Stats.Domain.Entitys.Shared;

namespace QuickBin.Infrastructure
{
    public class QuickBinDbContext : DbContext
    {
        public QuickBinDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(typeof(QuickBinDbContext)) ?? Assembly.GetExecutingAssembly());
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

            configurationBuilder.Properties<DateOnly>().HaveConversion<DateOnlyConverter>();
            configurationBuilder.Properties<TimeOnly>().HaveConversion<TimeOnlyConverter>();
            configurationBuilder.Properties<string>().HaveMaxLength(256);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        private const string CreatedFieldName = nameof(EntityBase.CreatedAt);

        private void UpdateTimestamps()
        {
            foreach (var entry in ChangeTracker.Entries().Where(entry => entry.State is EntityState.Added))
            {
                if (entry.Properties.Any(p => p.Metadata.Name == CreatedFieldName) && entry.State == EntityState.Added)
                    entry.Property(CreatedFieldName).CurrentValue = DateTime.UtcNow;
            }
        }

        public class QuickBinDbContextFactory : IDesignTimeDbContextFactory<QuickBinDbContext>
        {
            public QuickBinDbContext CreateDbContext(string[] args)
            {
                var optionsBuilder = new DbContextOptionsBuilder<QuickBinDbContext>();
                optionsBuilder.UseNpgsql();

                return new QuickBinDbContext(optionsBuilder.Options);
            }
        }
    }
}