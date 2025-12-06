using Microsoft.AspNetCore.Mvc;
using NotesApp.Core.Interfaces;
using NotesApp.Core.Models;

namespace NotesApp.Web.Controllers
{
    public class NotesController : Controller
    {
        private readonly INoteRepository _noteRepository;

        public NotesController(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        public async Task<IActionResult> Index()
        {
            var notes = await _noteRepository.GetAllNotes();
            return View(notes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title", "Content")] Note note)
        {
            if (ModelState.IsValid)
            {
                note.CreatedAt = DateTime.UtcNow;
                note.UpdatedAt = DateTime.UtcNow;
                await _noteRepository.AddNotes(note);
                return RedirectToAction(nameof(Index));
            }
            return View(note);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var note = await _noteRepository.GetNotesById(id);
            if (note == null) return NotFound();
            return View(note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content")] Note model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var note = await _noteRepository.GetNotesById(id);
            if (note == null) return NotFound();

            note.Title = model.Title;
            note.Content = model.Content;
            note.UpdatedAt = DateTime.UtcNow;
            await _noteRepository.UpdateNotes(note);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _noteRepository.DeleteNotes(id);
            return RedirectToAction(nameof(Index));
        }

        // AJAX inline update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContent([FromBody] InlineUpdateDto dto)
        {
            var note = await _noteRepository.GetNotesById(dto.Id);
            if (note == null) return NotFound();

            note.Title = dto.Title ?? string.Empty;
            note.Content = dto.Content ?? string.Empty;
            note.UpdatedAt = DateTime.UtcNow;

            await _noteRepository.UpdateNotes(note);
            return Json(new { success = true, updatedAt = note.UpdatedAt });
        }

        public class InlineUpdateDto
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public string? Content { get; set; }
        }
    }
}
