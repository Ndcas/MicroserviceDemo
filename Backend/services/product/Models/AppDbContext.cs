using Microsoft.EntityFrameworkCore;

namespace product.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductType> ProductTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("utf8mb4_0900_ai_ci").HasCharSet("utf8mb4");

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt).ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("products_ibfk_2");

            entity.HasOne(d => d.ProductType).WithMany(p => p.Products)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("products_ibfk_1");
        });

        modelBuilder.Entity<ProductType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasMany(d => d.Brands).WithMany(p => p.ProductTypes)
                .UsingEntity<Dictionary<string, object>>(
                    "BrandType",
                    r => r.HasOne<Brand>().WithMany().HasForeignKey("BrandId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("brand_type_ibfk_2"),
                    l => l.HasOne<ProductType>().WithMany().HasForeignKey("ProductTypeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("brand_type_ibfk_1"),
                    j =>
                    {
                        j.HasKey("ProductTypeId", "BrandId").HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                        j.ToTable("brand_type");

                        j.HasIndex(new[] { "BrandId" }, "brand_id");

                        j.IndexerProperty<int>("ProductTypeId").HasColumnName("product_type_id");

                        j.IndexerProperty<int>("BrandId").HasColumnName("brand_id");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
