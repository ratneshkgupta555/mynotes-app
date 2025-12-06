using Microsoft.EntityFrameworkCore;
using NotesApp.Core.Interfaces;
using NotesApp.Core.Models;
using NotesApp.Infrastructure.Data;

namespace NotesApp.Infrastructure.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly AppDbContext _dbcontext;

        public NoteRepository(AppDbContext appDbContext)
        {
            this._dbcontext = appDbContext;
        }
        public async Task AddNotes(Note note)
        {
            await _dbcontext.Notes.AddAsync(note);
            await _dbcontext.SaveChangesAsync();
        }

        public async Task DeleteNotes(int id)
        {
            var note = await _dbcontext.Notes.FindAsync(id);
            if(note != null)
            {
                _dbcontext.Notes.Remove(note);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Note>> GetAllNotes()
        {
            return await _dbcontext.Notes
                .OrderByDescending(note => note.CreatedAt).ToListAsync();
        }

        public async Task<Note?> GetNotesById(int id)
        {
            return await _dbcontext.Notes.FindAsync(id);
        }

        public async Task UpdateNotes(Note note)
        {
            _dbcontext.Notes.Update(note);
            await _dbcontext.SaveChangesAsync();
        }
    }
}
