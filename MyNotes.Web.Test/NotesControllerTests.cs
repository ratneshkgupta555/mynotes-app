namespace MyNotes.Web.Test
{
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using MyNotes.Core.Interfaces;
    using MyNotes.Core.Models;
    using MyNotes.Web.Controllers;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class NotesControllerTests
    {
        private readonly Mock<INoteRepository> _mockRepo;
        private readonly NotesController _controller;

        public NotesControllerTests()
        {
            _mockRepo = new Mock<INoteRepository>();
            _controller = new NotesController(_mockRepo.Object);
        }

        // Success Case - Validating Index Action Get Result
        [Fact]
        public async Task Index_ReturnViewResult()
        {
            // Arrange
            var notes = new List<Note>
            {
                new Note { Id = 1, Title = "Note 1", Content = "Content 1" },
                new Note { Id = 2, Title = "Note 2", Content = "Content 2" }
            };

            _mockRepo.Setup(r => r.GetAllNotes()).ReturnsAsync(notes);
            // Act
            var result = await _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(notes, result.Model);

        }
        // Error Case - Validating Index Action Get Result
        [Fact]
        public async Task Index_ReturnViewResult_OnException()
        {
            _mockRepo.Setup(r => r.GetAllNotes()).ThrowsAsync(new Exception("error on getting all notes"));

            // Assert
            Assert.ThrowsAsync<Exception>(() => _controller.Index());

        }


        // ============================
        // CREATE (POST)
        // ============================
        [Fact]
        public async Task Create_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var newNote = new Note { Title = "Test", Content = "Hello" };

            // Act
            var result = await _controller.Create(newNote);

            // Assert
            _mockRepo.Verify(r => r.AddNotes(It.IsAny<Note>()), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        // ============================
        // ERROR CASE - CREATE (POST)
        // ============================
        [Fact]
        public async Task Create_ValidModel_RedirectsToIndex_OnException()
        {
            // Arrange
            var newNote = new Note { Title = "Test", Content = "Hello" };

            _mockRepo.Setup(r => r.AddNotes(It.IsAny<Note>())).ThrowsAsync(new Exception("error on adding note"));

            // Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.Create(newNote));
            Assert.Equal("error on adding note", ex.Message);
            _mockRepo.Verify(r => r.AddNotes(It.IsAny<Note>()), Times.Once);
        }

        [Fact]
        public async Task Create_InvalidModel_ReturnsView()
        {
            // Arrange
            var newNote = new Note();
            _controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = await _controller.Create(newNote);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(newNote, view.Model);
        }

        // ============================
        // EDIT (GET)
        // ============================
        [Fact]
        public async Task Edit_Get_ReturnsView_WhenNoteExists()
        {
            // Arrange
            var note = new Note { Id = 1 };
            _mockRepo.Setup(r => r.GetNotesById(1)).ReturnsAsync(note);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(note, view.Model);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenNoteDoesNotExist()
        {
            _mockRepo.Setup(r => r.GetNotesById(1)).ReturnsAsync((Note?)null);

            var result = await _controller.Edit(1);

            Assert.IsType<NotFoundResult>(result);
        }

        // ============================
        // EDIT (POST)
        // ============================
        [Fact]
        public async Task Edit_Post_ReturnsBadRequest_WhenIdMismatch()
        {
            var model = new Note { Id = 2 };

            var result = await _controller.Edit(1, model);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsView()
        {
            var model = new Note { Id = 1 };
            _controller.ModelState.AddModelError("Title", "Required");

            var result = await _controller.Edit(1, model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, view.Model);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_UpdatesNote_AndRedirects()
        {
            // Arrange
            var existing = new Note { Id = 1, Title = "Old", Content = "Old" };
            var updated = new Note { Id = 1, Title = "New", Content = "New" };

            _mockRepo.Setup(r => r.GetNotesById(1)).ReturnsAsync(existing);

            // Act
            var result = await _controller.Edit(1, updated);

            // Assert
            _mockRepo.Verify(r => r.UpdateNotes(It.IsAny<Note>()), Times.Once);
            Assert.IsType<RedirectToActionResult>(result);
        }

        // ============================
        // DELETE
        // ============================
        [Fact]
        public async Task Delete_RemovesNote_AndRedirects()
        {
            // Act
            var result = await _controller.Delete(1);

            // Assert
            _mockRepo.Verify(r => r.DeleteNotes(1), Times.Once);
            Assert.IsType<RedirectToActionResult>(result);
        }

        // ============================
        // UPDATE CONTENT (AJAX)
        // ============================
        [Fact]
        public async Task UpdateContent_ReturnsNotFound_WhenNoteMissing()
        {
            _mockRepo.Setup(r => r.GetNotesById(1)).ReturnsAsync((Note?)null);

            var dto = new NotesController.InlineUpdateDto { Id = 1 };

            var result = await _controller.UpdateContent(dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateContent_UpdatesNote_AndReturnsJson()
        {
            // Arrange
            var note = new Note { Id = 1, Title = "Old", Content = "Old" };
            _mockRepo.Setup(r => r.GetNotesById(1)).ReturnsAsync(note);

            var dto = new NotesController.InlineUpdateDto
            {
                Id = 1,
                Title = "New",
                Content = "New"
            };

            // Act
            var result = await _controller.UpdateContent(dto);

            // Assert
            _mockRepo.Verify(r => r.UpdateNotes(It.IsAny<Note>()), Times.Once);

            var json = Assert.IsType<JsonResult>(result);
            Assert.NotNull(json.Value);
        }

    }
}