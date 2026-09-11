using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Database;

public partial class ProductServiceContext : DbContext
{
    public ProductServiceContext() { }

    public ProductServiceContext(DbContextOptions<ProductServiceContext> options) : base(options) { }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<InboxMessage> InboxMessages { get; set; }

    public virtual DbSet<OutboxMessage> OutboxMessages { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductType> ProductTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("utf8mb4_0900_ai_ci").HasCharSet("utf8mb4");

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("brands");

            entity.Property(e => e.Id).HasColumnName("id");

            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");

            entity
                .Property(e => e.Image)
                .HasMaxLength(255)
                .HasColumnName("image");

            entity
                .Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<InboxMessage>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PRIMARY");

            entity.ToTable("inbox_messages");

            entity.Property(e => e.EventId).HasColumnName("event_id");

            entity
                .Property(e => e.ProcessedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("processed_at");
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("outbox_messages");

            entity.HasIndex(e => e.EventId, "event_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");

            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");

            entity.Property(e => e.EventId).HasColumnName("event_id");

            entity
                .Property(e => e.Payload)
                .HasColumnType("text")
                .HasColumnName("payload");

            entity
                .Property(e => e.PublishedAt)
                .HasColumnType("timestamp")
                .HasColumnName("published_at");

            entity
                .Property(e => e.Topic)
                .HasMaxLength(255)
                .HasColumnName("topic");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("products");

            entity.HasIndex(e => e.BrandId, "brand_id");

            entity.HasIndex(e => e.ProductTypeId, "product_type_id");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.BrandId).HasColumnName("brand_id");

            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");

            entity
                .Property(e => e.Image)
                .HasMaxLength(255)
                .HasColumnName("image");

            entity
                .Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity
                .Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");

            entity.Property(e => e.ProductTypeId).HasColumnName("product_type_id");

            entity
                .Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity
                .HasOne(d => d.Brand)
                .WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("products_ibfk_2");

            entity
                .HasOne(d => d.ProductType)
                .WithMany(p => p.Products)
                .HasForeignKey(d => d.ProductTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("products_ibfk_1");
        });

        modelBuilder.Entity<ProductType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("product_types");

            entity.Property(e => e.Id).HasColumnName("id");

            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");

            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity
                .HasMany(d => d.Brands)
                .WithMany(p => p.ProductTypes)
                .UsingEntity<Dictionary<string, object>>(
                    "BrandType",
                    r => r
                        .HasOne<Brand>()
                        .WithMany()
                        .HasForeignKey("BrandId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("brand_type_ibfk_2"),
                    l => l
                        .HasOne<ProductType>()
                        .WithMany()
                        .HasForeignKey("ProductTypeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("brand_type_ibfk_1"),
                    j =>
                    {
                        j
                            .HasKey("ProductTypeId", "BrandId")
                            .HasName("PRIMARY")
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
