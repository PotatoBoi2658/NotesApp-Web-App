using System.ComponentModel.DataAnnotations;

namespace NotesApp.Models
{
    /// <summary>
    /// Represents a tag that can be attached to notes.
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Tag name. Required, max length 50.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Navigation property for notes associated with this tag.
        /// </summary>
        public ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
    }
}