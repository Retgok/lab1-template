using Microsoft.EntityFrameworkCore;

public class PersonDb : DbContext
{
    public PersonDb(DbContextOptions<PersonDb> options) : base(options) { }

    public DbSet<Person> Persons => Set<Person>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    modelBuilder.Entity<Person>(entity =>
    {
        entity.ToTable("persons");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.Name).HasColumnName("name").IsRequired();
        entity.Property(e => e.Age).HasColumnName("age");
        entity.Property(e => e.Address).HasColumnName("address");
        entity.Property(e => e.Work).HasColumnName("work");
    });

    base.OnModelCreating(modelBuilder);
    }
}
