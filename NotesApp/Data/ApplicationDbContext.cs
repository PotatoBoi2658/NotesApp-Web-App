using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NotesApp.Models;

namespace NotesApp.Data
{
    /// <summary>
    /// EF Core DbContext for the NotesApp, includes Identity and application DbSets.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="options">The options to be used by this DbContext.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Notes table.
        /// </summary>
        public DbSet<Note> Notes { get; set; }

        /// <summary>
        /// Tags table.
        /// </summary>
        public DbSet<Tag> Tags { get; set; }

        /// <summary>
        /// Join table between notes and tags.
        /// </summary>
        public DbSet<NoteTag> NoteTags { get; set; }

        /// <summary>
        /// Configure model mappings and relationships.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // composite primary key for the join table
            modelBuilder.Entity<NoteTag>()
                .HasKey(nt => new { nt.NoteId, nt.TagId });

            // explicit relationships
            modelBuilder.Entity<NoteTag>()
                .HasOne(nt => nt.Note)
                .WithMany(n => n.NoteTags)
                .HasForeignKey(nt => nt.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NoteTag>()
                .HasOne(nt => nt.Tag)
                .WithMany(t => t.NoteTags)
                .HasForeignKey(nt => nt.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}