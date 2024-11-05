using Microsoft.EntityFrameworkCore;

class AppDB : DbContext
{
    public AppDB(DbContextOptions<AppDB> options)
        : base(options) { }

    public DbSet<Gender> Genders => Set<Gender>();
    public DbSet<Hobby> Hobbies => Set<Hobby>();
    public DbSet<Personal> Personals => Set<Personal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Gender>().HasIndex(g => g.Name).IsUnique();

        modelBuilder.Entity<Hobby>().HasIndex(h => h.Name).IsUnique();
    }
}
