using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Database;

public partial class OrderServiceContext : DbContext
{
    public OrderServiceContext() { }

    public OrderServiceContext(DbContextOptions<OrderServiceContext> options) : base(options) { }

    public virtual DbSet<InboxMessage> InboxMessages { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<OutboxMessage> OutboxMessages { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("utf8mb4_0900_ai_ci").HasCharSet("utf8mb4");

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

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("orders");

            entity.Property(e => e.Id).HasColumnName("id");

            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");

            entity
                .Property(e => e.Status)
                .HasDefaultValueSql("'PROCESSING'")
                .HasColumnType("enum('PROCESSING','UNPAID','CONFIRMED','FINISHED','CANCELED')")
                .HasColumnName("status");

            entity
                .Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity
                .HasKey(e => new { e.OrderId, e.ProductId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("order_details");

            entity.Property(e => e.OrderId).HasColumnName("order_id");

            entity.Property(e => e.ProductId).HasColumnName("product_id");

            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");

            entity
                .Property(e => e.PriceAtBooking)
                .HasPrecision(10, 2)
                .HasColumnName("price_at_booking");

            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity
                .HasOne(d => d.Order)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order_details_ibfk_1");
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

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("transactions");

            entity.HasIndex(e => e.OrderId, "order_id");

            entity.Property(e => e.Id).HasColumnName("id");

            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");

            entity.Property(e => e.OrderId).HasColumnName("order_id");

            entity
                .Property(e => e.Payload)
                .HasColumnType("text")
                .HasColumnName("payload");

            entity
                .Property(e => e.Status)
                .HasColumnType("enum('SUCCESS','FAIL')")
                .HasColumnName("status");

            entity
                .Property(e => e.TransactionRef)
                .HasMaxLength(255)
                .HasColumnName("transaction_ref");

            entity
                .Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity
                .HasOne(d => d.Order)
                .WithMany(p => p.Transactions)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactions_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
