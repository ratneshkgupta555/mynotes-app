using System.ComponentModel.DataAnnotations;

namespace NotesApp.Core.Models
{
    public class Note
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; }
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
