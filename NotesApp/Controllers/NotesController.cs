using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NotesApp.Data;
using NotesApp.Models;
using NotesApp.Models.ViewModels;
using System.Security.Claims;


namespace NotesApp.Controllers
{ /// <summary>
  /// Controller for CRUD operations on notes.
  /// </summary>
    [Authorize]
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        /// <summary>
        /// Creates a new <see cref="NotesController"/>.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public NotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: Lists notes for the current user or all notes for administrators.
        /// </summary>
        /// <returns>The index view with notes.</returns>
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Administrator");

            IQueryable<Note> notes = _context.Notes
                .Include(n => n.User) // include the note owner so view can show username
                .Include(n => n.NoteTags)
                    .ThenInclude(nt => nt.Tag);

            if (!isAdmin)
            {
                notes = notes.Where(n => n.UserId == userId);
            }

            return View(await notes.ToListAsync());
        }


        /// <summary>
        /// GET: Details for a single note.
        /// </summary>
        /// <param name="id">Note identifier.</param>
        /// <returns>The details view or NotFound/Forbid as appropriate.</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes
                .Include(n => n.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (note == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (note.UserId != userId && !User.IsInRole("Administrator"))
            {
                return Forbid();
            }

            return View(note);
        }

        /// <summary>
        /// GET: Show create note form.
        /// </summary>
        /// <returns>Create view.</returns>
        public IActionResult Create()
        {
            return View(new CreateNoteViewModel());
        }

        /// <summary>
        /// POST: Create a new note for the current user and attach tags.
        /// </summary>
        /// <param name="model">The create note view model.</param>
        /// <returns>Redirects to Index on success, otherwise returns the view with validation errors.</returns>
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNoteViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var note = new Note
            {
                Title = model.Title,
                Content = model.Content,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                CreatedAt = DateTime.Now
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync(); // IMPORTANT: we need note.Id

            // ---- TAG HANDLING STARTS HERE ----
            if (!string.IsNullOrWhiteSpace(model.Tags))
            {
                var tagNames = model.Tags
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim().ToLower())
                    .Distinct();

                foreach (var tagName in tagNames)
                {
                    // Check if tag exists
                    var tag = await _context.Tags
                        .FirstOrDefaultAsync(t => t.Name == tagName);

                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName };
                        _context.Tags.Add(tag);
                        await _context.SaveChangesAsync();
                    }

                    // Create link in NoteTag
                    var noteTag = new NoteTag
                    {
                        NoteId = note.Id,
                        TagId = tag.Id
                    };

                    _context.NoteTags.Add(noteTag);
                }

                await _context.SaveChangesAsync();
            }
            // ---- TAG HANDLING ENDS HERE ----

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// GET: Edit form for a note.
        /// </summary>
        /// <param name="id">Note identifier.</param>
        /// <returns>Edit view or NotFound/Forbid as appropriate.</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes.FindAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (note.UserId != userId && !User.IsInRole("Administrator"))
            {
                return Forbid();
            }

            if (note == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", note.UserId);
            return View(note);
        }

        /// <summary>
        /// POST: Apply edits to a note. Only title and content are updatable.
        /// </summary>
        /// <param name="input">Note input containing Id, Title and Content.</param>
        /// <returns>Redirects to Index on success or NotFound/Forbid.</returns>
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Title,Content")] Note input)
        {
            // Get the real note from DB
            var note = await _context.Notes.FindAsync(input.Id);
            if (note == null)
                return NotFound();

            // Authorization check
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (note.UserId != userId && !User.IsInRole("Administrator"))
                return Forbid();

            // Update allowed fields only
            note.Title = input.Title;
            note.Content = input.Content;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Notes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes
                .Include(n => n.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (note.UserId != userId && !User.IsInRole("Administrator"))
            {
                return Forbid();
            }
            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }

        // POST: Notes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note != null)
            {
                _context.Notes.Remove(note);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NoteExists(int id)
        {
            return _context.Notes.Any(e => e.Id == id);
        }
        public async Task<IActionResult> ByTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return RedirectToAction(nameof(Index));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Administrator");

            var notesQuery = _context.Notes
                .Include(n => n.NoteTags)
                    .ThenInclude(nt => nt.Tag)
                .Where(n => n.NoteTags.Any(nt => nt.Tag.Name == tag));

            if (!isAdmin)
            {
                notesQuery = notesQuery.Where(n => n.UserId == userId);
            }

            ViewBag.Tag = tag;
            return View(await notesQuery.ToListAsync());
        }

        public async Task<IActionResult> BrowseTags()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Administrator");

            IQueryable<Tag> tags;

            if (isAdmin)
            {
                tags = _context.Tags;
            }
            else
            {
                tags = _context.NoteTags
                    .Where(nt => nt.Note.UserId == userId)
                    .Select(nt => nt.Tag)
                    .Distinct();
            }

            return View(await tags.OrderBy(t => t.Name).ToListAsync());
        }


        private bool IsOwnerOrAdmin(Note note)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return note.UserId == userId || User.IsInRole("Administrator");
        }

    }
}
