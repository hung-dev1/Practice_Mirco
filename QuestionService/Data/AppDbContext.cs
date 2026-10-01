using Microsoft.EntityFrameworkCore;
using QuestionService.Entity;

namespace QuestionService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Question> Questions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Question>(entity =>
            {
                entity.ToTable("question");
                entity.HasKey(q => q.Id);
                entity.Property(q => q.Id)
                      .UseIdentityAlwaysColumn();

                entity.Property(q => q.Title)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.Property(q => q.OptionA)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(q => q.OptionB)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(q => q.OptionC)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(q => q.OptionD)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(q => q.CorrectAnswer)
                      .IsRequired()
                      .HasMaxLength(1)
                      .HasColumnType("char(1)");
            });
        }
    }
}
