using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // USER relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.OrganisedEvents)
                .WithOne(e => e.Organiser)
                .HasForeignKey(e => e.OrganiserID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Enrolments)
                .WithOne(e => e.Participant)
                .HasForeignKey(e => e.ParticipantID)
                .OnDelete(DeleteBehavior.NoAction);

            // EVENT relationships
            modelBuilder.Entity<Event>()
                .HasOne(e => e.EventType)
                .WithMany(et => et.Events)
                .HasForeignKey(e => e.EventTypeID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Event>()
                .HasMany(e => e.Categories)
                .WithOne(c => c.Event)
                .HasForeignKey(c => c.EventID)
                .OnDelete(DeleteBehavior.NoAction);

            // CATEGORY relationships
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Enrolments)
                .WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.NoAction);

            // ENROLMENT and RESULT relationship
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Result)
                .WithOne(r => r.Enrolment)
                .HasForeignKey<Result>(r => r.EnrolmentID)
                .OnDelete(DeleteBehavior.NoAction);

            // Map tables explicitly
            modelBuilder.Entity<User>().ToTable("USER", "dbo");
            modelBuilder.Entity<Event>().ToTable("EVENT", "dbo");
            modelBuilder.Entity<EventType>().ToTable("EVENT TYPE", "dbo");
            modelBuilder.Entity<Category>().ToTable("CATEGORY", "dbo");
            modelBuilder.Entity<Enrolment>().ToTable("ENROLMENT", "dbo");
            modelBuilder.Entity<Result>().ToTable("RESULT", "dbo");
        }
    }
}