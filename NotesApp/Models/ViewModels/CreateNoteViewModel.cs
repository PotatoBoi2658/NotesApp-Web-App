using System.ComponentModel.DataAnnotations;

namespace NotesApp.Models.ViewModels
{
    public class CreateNoteViewModel
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        public string? Content { get; set; }

        // comma-separated tags: "school, linux, project"
        public string? Tags { get; set; }
    }
}