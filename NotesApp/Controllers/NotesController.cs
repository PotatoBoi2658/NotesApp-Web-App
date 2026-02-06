using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NotesApp.Data;
using NotesApp.Models;
using NotesApp.Models.ViewModels;
using System.Security.Claims;


namespace NotesApp.Controllers
{
    [Authorize]
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Notes
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Administrator");

            IQueryable<Note> notes = _context.Notes
                .Include(n => n.NoteTags)
                    .ThenInclude(nt => nt.Tag);

            if (!isAdmin)
            {
                notes = notes.Where(n => n.UserId == userId);
            }

            return View(await notes.ToListAsync());
        }


        // GET: Notes/Details/5
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

        // GET: Notes/Create
        public IActionResult Create()
        {
            return View(new CreateNoteViewModel());
        }


        // POST: Notes/Create
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




        // GET: Notes/Edit/5
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

        // POST: Notes/Edit/5
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
