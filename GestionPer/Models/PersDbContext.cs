using GestionPer.Models;
using Microsoft.EntityFrameworkCore;

public class PersDbContext : DbContext
{
    public PersDbContext(DbContextOptions<PersDbContext> options)
    : base(options) { }

    public DbSet<Employees> Employees { get; set; }
    public DbSet<Holiday> Holidays { get; set; }
    public DbSet<Malady> Maladies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Fluent API configurations (if needed)
        modelBuilder.Entity<Employees>().ToTable("Employees");
        modelBuilder.Entity<Holiday>().ToTable("Holidays");
        modelBuilder.Entity<Malady>().ToTable("Maladies");
    }
}
