using Microsoft.EntityFrameworkCore;
using QuizService.Entity;

namespace QuizService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Quiz config
            modelBuilder.Entity<Quiz>(entity =>
            {
                entity.ToTable("quiz");
                entity.HasKey(q => q.Id);
                entity.Property(q => q.Id)
                      .UseIdentityAlwaysColumn();

                entity.Property(q => q.Title)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(q => q.Description)
                      .HasColumnType("text");

                entity.Property(q => q.Duration)
                      .IsRequired();

                entity.Property(q => q.CreatedBy)
                      .IsRequired();

                entity.Property(q => q.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(q => q.IsActive)
                      .IsRequired()
                      .HasDefaultValue(true);
            });

            // QuizQuestion config
            modelBuilder.Entity<QuizQuestion>(entity =>
            {
                entity.ToTable("quiz_question");
                entity.HasKey(qq => qq.Id);
                entity.Property(qq => qq.Id)
                      .UseIdentityAlwaysColumn();

                entity.Property(qq => qq.QuizId)
                      .IsRequired();

                entity.Property(qq => qq.QuestionId)
                      .IsRequired();

                entity.Property(qq => qq.QuestionOrder)
                      .IsRequired();

                // Unique constraint: (quiz_id, question_id)
                entity.HasIndex(qq => new { qq.QuizId, qq.QuestionId })
                      .IsUnique()
                      .HasDatabaseName("uq_quiz_question");

                // FK: quiz_question.quiz_id -> quiz.id (CASCADE DELETE)
                entity.HasOne(qq => qq.Quiz)
                      .WithMany(q => q.QuizQuestions)
                      .HasForeignKey(qq => qq.QuizId)
                      .HasConstraintName("fk_quiz_question_quiz")
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
