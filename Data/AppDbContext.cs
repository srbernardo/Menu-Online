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
    public DbSet<Order> Orders { get; set; }
    public DbSet<RateTime> RateTimes { get; set; }
    public DbSet<User> Users { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Address>(e =>
    {
      e.Property(a => a.state)
       .HasConversion<string>();

      e.HasOne(a => a.Client)
       .WithMany(c => c.Addresses)
       .HasForeignKey(a => a.ClientId)
       .OnDelete(DeleteBehavior.Cascade);

      e.HasOne(a => a.User)
       .WithOne(u => u.Address)
       .HasForeignKey<Address>(a => a.UserId)
       .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<BusinessHours>(e =>
    {
      e.HasOne(b => b.User)
       .WithOne(u => u.BusinessHours)
       .HasForeignKey<BusinessHours>(b => b.UserId)
       .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<Category>(e =>
    {
      e.HasMany(c => c.Items);
    });

    modelBuilder.Entity<Client>(e =>
    {
      e.HasMany(c => c.Orders)
       .WithOne(o => o.Client)
       .HasForeignKey(o => o.ClientId)
       .OnDelete(DeleteBehavior.Cascade);

      e.HasMany(c => c.Addresses)
       .WithOne(a => a.Client)
       .HasForeignKey(a => a.ClientId)
       .OnDelete(DeleteBehavior.Cascade);
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

      e.HasOne(io => io.Item)
       .WithMany(i => i.ItemOrders)
       .HasForeignKey(io => io.ItemId)
       .OnDelete(DeleteBehavior.Cascade);

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
       .OnDelete(DeleteBehavior.Cascade);

      e.HasOne(i => i.User)
       .WithMany(u => u.Items)
       .HasForeignKey(i => i.UserId)
       .OnDelete(DeleteBehavior.Cascade);
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
      e.Property(u => u.Role)
       .HasConversion<string>();

      e.Property(u => u.Status)
       .HasConversion<string>();

      e.HasMany(u => u.Items)
       .WithOne(i => i.User)
       .HasForeignKey(i => i.UserId)
       .OnDelete(DeleteBehavior.Cascade);

      e.HasMany(u => u.Orders)
       .WithOne(o => o.User)
       .HasForeignKey(o => o.UserId)
       .OnDelete(DeleteBehavior.Cascade);

      e.HasMany(u => u.RateTimes)
       .WithOne(rt => rt.User)
       .HasForeignKey(rt => rt.UserId)
       .OnDelete(DeleteBehavior.Cascade);

      e.HasOne(u => u.Address)
       .WithOne(a => a.User)
       .HasForeignKey<User>(u => u.AddressId)
       .OnDelete(DeleteBehavior.Cascade);

      e.HasOne(u => u.BusinessHours)
       .WithOne(b => b.User)
       .HasForeignKey<User>(u => u.BusinessHoursId)
       .OnDelete(DeleteBehavior.Cascade);
    });
  }
}
