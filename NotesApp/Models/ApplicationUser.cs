using Microsoft.AspNetCore.Identity;

namespace NotesApp.Models

{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
