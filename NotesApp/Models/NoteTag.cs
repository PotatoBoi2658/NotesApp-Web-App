namespace NotesApp.Models
{
    /// <summary>
    /// Join entity for the many-to-many relationship between <see cref="Note"/> and <see cref="Tag"/>.
    /// </summary>
    public class NoteTag
    {
        /// <summary>
        /// Note primary key.
        /// </summary>
        public int NoteId { get; set; }

        /// <summary>
        /// Navigation to the <see cref="Note"/>.
        /// </summary>
        public Note Note { get; set; } = null!;

        /// <summary>
        /// Tag primary key.
        /// </summary>
        public int TagId { get; set; }

        /// <summary>
        /// Navigation to the <see cref="Tag"/>.
        /// </summary>
        public Tag Tag { get; set; } = null!;
    }
}