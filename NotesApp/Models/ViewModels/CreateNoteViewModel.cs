using System.ComponentModel.DataAnnotations;

namespace NotesApp.Models.ViewModels
{
    /// <summary>
    /// View model used to create a new note via the UI.
    /// </summary>
    public class CreateNoteViewModel
    {
        /// <summary>
        /// Note title. Required and limited to 200 characters.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        /// <summary>
        /// Optional note content/body.
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Comma-separated list of tag names (e.g. "school, linux, project").
        /// </summary>
        public string? Tags { get; set; }
    }
}