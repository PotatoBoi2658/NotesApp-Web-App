using System.ComponentModel.DataAnnotations;

namespace NotesApp.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;

        public ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
    }
}
