using IaAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IaAgent.Infrastructure.Persistence;

public class ArbitrageDbContext : DbContext
{
    public ArbitrageDbContext(DbContextOptions<ArbitrageDbContext> options) : base(options) { }

    public DbSet<MarketOpportunity> Opportunities { get; set; }
    public DbSet<ChineseProduct> ChineseProducts { get; set; }
    public DbSet<ArbitrageCalculation> ArbitrageCalculations { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<MercadoLibreListing> Listings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MarketOpportunity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.MercadoLibreProductId).IsUnique();
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.DemandScore);
            e.HasMany(x => x.ChineseProducts)
             .WithOne()
             .HasForeignKey(x => x.OpportunityId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Calculation)
             .WithOne()
             .HasForeignKey<ArbitrageCalculation>(x => x.OpportunityId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChineseProduct>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.OpportunityId, x.ExternalProductId, x.SourcePlatform }).IsUnique();
        });

        modelBuilder.Entity<ArbitrageCalculation>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.IsProfitable);
        });

        modelBuilder.Entity<PurchaseOrder>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.OpportunityId);
        });

        modelBuilder.Entity<MercadoLibreListing>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.MercadoLibreListingId);
            e.HasIndex(x => x.Status);
        });
    }
}
