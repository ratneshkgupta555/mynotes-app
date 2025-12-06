using NotesApp.Core.Models;

namespace NotesApp.Core.Interfaces
{
    public interface INoteRepository
    {
        Task<IEnumerable<Note>> GetAllNotes();
        Task<Note?> GetNotesById(int id);
        Task AddNotes(Note note);
        Task UpdateNotes(Note note);
        Task DeleteNotes(int id);
    }
}
