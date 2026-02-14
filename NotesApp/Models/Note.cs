using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotesApp.Models
{
    /// <summary>
    /// Represents a user-created note.
    /// </summary>
    public class Note
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Note title. Required, max length 200.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        /// <summary>
        /// Optional note body/content.
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Creation timestamp (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Foreign key to the owning user.
        /// </summary>
        [Required]
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Navigation property to the owning <see cref="ApplicationUser"/>.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Many-to-many join entries connecting this note to tags.
        /// </summary>
        public ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
    }
}