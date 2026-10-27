namespace MenuOnline.Data;

using MenuOnline.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
  {
  }

  public DbSet<Address> Addresses { get; set; }
  public DbSet<BusinessHours> BusinessHours { get; set; }
  public DbSet<Category> Categories { get; set; }
  public DbSet<Client> Clients { get; set; }
  public DbSet<Extra> Extras { get; set; }
  public DbSet<ItemExtra> ItemExtras { get; set; }
  public DbSet<ItemOrder> ItemOrders { get; set; }
  public DbSet<Item> Items { get; set; }
  public DbSet<MediaAttachment> MediaAttachments { get; set; }
  public DbSet<MediaBlob> MediaBlobs { get; set; }
  public DbSet<Order> Orders { get; set; }
  public DbSet<RateTime> RateTimes { get; set; }
  public DbSet<User> Users { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Address>(e =>
    {
      e.Property(a => a.State).HasConversion<string>();

      // Cliente tem varios enderecos
      e.HasOne(a => a.Client)
       .WithMany(c => c.Addresses)
       .HasForeignKey(a => a.ClientId)
       .OnDelete(DeleteBehavior.Cascade);

      // User tem exatamente um endereco (FK em Address.UserId, unica)
      e.HasOne(a => a.User)
       .WithOne(u => u.Address)
       .HasForeignKey<Address>(a => a.UserId)
       .OnDelete(DeleteBehavior.Cascade);

      e.HasIndex(a => a.UserId).IsUnique();
    });

    modelBuilder.Entity<BusinessHours>(e =>
    {
      e.HasOne(b => b.User)
       .WithOne(u => u.BusinessHours)
       .HasForeignKey<BusinessHours>(b => b.UserId)
       .OnDelete(DeleteBehavior.Cascade);

      e.HasIndex(b => b.UserId).IsUnique();
    });

    modelBuilder.Entity<Client>(e =>
    {
      // Nao apaga pedidos (historico) ao apagar o cliente
      e.HasMany(c => c.Orders)
       .WithOne(o => o.Client)
       .HasForeignKey(o => o.ClientId)
       .OnDelete(DeleteBehavior.Restrict);
    });

    modelBuilder.Entity<ItemExtra>(e =>
    {
      e.HasKey(ie => new { ie.ItemId, ie.ExtraId });

      e.HasOne(ie => ie.Item)
       .WithMany(i => i.ItemExtras)
       .HasForeignKey(ie => ie.ItemId)
       .OnDelete(DeleteBehavior.Cascade);

      e.HasOne(ie => ie.Extra)
       .WithMany(x => x.ItemExtras)
       .HasForeignKey(ie => ie.ExtraId)
       .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<ItemOrder>(e =>
    {
      e.HasKey(io => new { io.ItemId, io.OrderId });

      // Item vendido nao pode ser apagado (preserva historico)
      e.HasOne(io => io.Item)
       .WithMany(i => i.ItemOrders)
       .HasForeignKey(io => io.ItemId)
       .OnDelete(DeleteBehavior.Restrict);

      e.HasOne(io => io.Order)
       .WithMany(o => o.ItemOrders)
       .HasForeignKey(io => io.OrderId)
       .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<Item>(e =>
    {
      e.HasOne(i => i.Category)
       .WithMany(c => c.Items)
       .HasForeignKey(i => i.CategoryId)
       .OnDelete(DeleteBehavior.Restrict);

      e.HasOne(i => i.User)
       .WithMany(u => u.Items)
       .HasForeignKey(i => i.UserId)
       .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<Order>(e =>
    {
      e.Property(o => o.Status).HasConversion<string>();

      e.HasOne(o => o.User)
       .WithMany(u => u.Orders)
       .HasForeignKey(o => o.UserId)
       .OnDelete(DeleteBehavior.Restrict);

      e.HasOne(o => o.DeliveryAddress)
       .WithMany()
       .HasForeignKey(o => o.DeliveryAddressId)
       .OnDelete(DeleteBehavior.SetNull);
    });

    modelBuilder.Entity<RateTime>(e =>
    {
      e.HasOne(rt => rt.User)
       .WithMany(u => u.RateTimes)
       .HasForeignKey(rt => rt.UserId)
       .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<User>(e =>
    {
      e.Property(u => u.Role).HasConversion<string>();
      e.Property(u => u.Status).HasConversion<string>();
      e.HasIndex(u => u.Email).IsUnique();
      e.HasIndex(u => u.Slug).IsUnique();
    });

    modelBuilder.Entity<MediaBlob>(e =>
    {
      e.HasIndex(b => b.Key).IsUnique();
    });

    modelBuilder.Entity<MediaAttachment>(e =>
    {
      // Um anexo por dono + finalidade (equivale ao has_one_attached)
      e.HasIndex(a => new { a.RecordType, a.RecordId, a.Name }).IsUnique();

      e.HasOne(a => a.Blob)
       .WithMany(b => b.Attachments)
       .HasForeignKey(a => a.BlobId)
       .OnDelete(DeleteBehavior.Cascade);
    });
  }
}
