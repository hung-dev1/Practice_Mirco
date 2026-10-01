using AuthService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Role config
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Id)
                      .UseIdentityAlwaysColumn();
                entity.Property(r => r.Name)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.HasIndex(r => r.Name)
                      .IsUnique();

                // Seed data
                entity.HasData(
                    new Role { Id = 1, Name = "ADMIN" },
                    new Role { Id = 2, Name = "STUDENT" },
                    new Role { Id = 3, Name = "TEACHER" }
                );
            });

            // User config
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id)
                      .UseIdentityAlwaysColumn();
                entity.Property(u => u.Username)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(u => u.Password)
                      .IsRequired()
                      .HasMaxLength(255);
                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(150);
                entity.HasIndex(u => u.Username)
                      .IsUnique();
                entity.HasIndex(u => u.Email)
                      .IsUnique();

                entity.Property(u => u.IsActive)
                      .IsRequired()
                      .HasDefaultValue(true);

                // Foreign key: users.role_id -> roles.id
                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
