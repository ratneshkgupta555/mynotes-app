using Microsoft.AspNetCore.Mvc;
using MyNotes.Core.Interfaces;
using MyNotes.Core.Models;
using MyNotes.Web.Controllers;

namespace MyNotes.Test
{
    public class NotesControllerTests
    {
        [Fact]
        private readonly Mock<INoteRepository> _repoMock;
        private readonly NotesController _controller;

        public NotesControllerTests()
        {
            _repoMock = new Mock<INoteRepository>();
            _controller = new NotesController(_repoMock.Object);
        }

        [Fact]
        public async Task Index_ReturnsViewWithNotes()
        {
            var notes = new List<Note> { new Note { Id = 1, Title = "a", Content = "b" } };
            _repoMock.Setup(r => r.GetAllNotes()).ReturnsAsync(notes);

            var result = await _controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(notes, view.Model);
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsAndAddsNote()
        {
            var note = new Note { Title = "T", Content = "C" };
            _repoMock.Setup(r => r.AddNotes(It.IsAny<Note>())).Returns(Task.CompletedTask);

            var result = await _controller.Create(note);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.Index), redirect.ActionName);
            _repoMock.Verify(r => r.AddNotes(It.Is<Note>(n => n.Title == "T" && n.Content == "C" && n.CreatedAt != default && n.UpdatedAt != default)), Times.Once);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewWithModel()
        {
            _controller.ModelState.AddModelError("Title", "Required");
            var note = new Note { Title = "", Content = "C" };

            var result = await _controller.Create(note);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(note, view.Model);
            _repoMock.Verify(r => r.AddNotes(It.IsAny<Note>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Get_NotFound_WhenNoteMissing()
        {
            _repoMock.Setup(r => r.GetNotesById(5)).ReturnsAsync((Note?)null);

            var result = await _controller.Edit(5);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsView_WhenNoteExists()
        {
            var note = new Note { Id = 2, Title = "x", Content = "y" };
            _repoMock.Setup(r => r.GetNotesById(2)).ReturnsAsync(note);

            var result = await _controller.Edit(2);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(note, view.Model);
        }

        [Fact]
        public async Task Edit_Post_IdMismatch_ReturnsBadRequest()
        {
            var model = new Note { Id = 3, Title = "t", Content = "c" };

            var result = await _controller.Edit(4, model);

            Assert.IsType<BadRequestResult>(result);
            _repoMock.Verify(r => r.UpdateNotes(It.IsAny<Note>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsViewWithoutUpdating()
        {
            var model = new Note { Id = 1, Title = "", Content = "c" };
            _controller.ModelState.AddModelError("Title", "Required");

            var result = await _controller.Edit(1, model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(model, view.Model);
            _repoMock.Verify(r => r.UpdateNotes(It.IsAny<Note>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_NotFound_WhenExistingNoteMissing()
        {
            var model = new Note { Id = 7, Title = "t", Content = "c" };
            _repoMock.Setup(r => r.GetNotesById(7)).ReturnsAsync((Note?)null);

            var result = await _controller.Edit(7, model);

            Assert.IsType<NotFoundResult>(result);
            _repoMock.Verify(r => r.UpdateNotes(It.IsAny<Note>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_Valid_UpdatesAndRedirects()
        {
            var existing = new Note { Id = 9, Title = "old", Content = "old" };
            var model = new Note { Id = 9, Title = "new", Content = "new" };

            _repoMock.Setup(r => r.GetNotesById(9)).ReturnsAsync(existing);
            _repoMock.Setup(r => r.UpdateNotes(It.IsAny<Note>())).Returns(Task.CompletedTask);

            var result = await _controller.Edit(9, model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.Index), redirect.ActionName);

            _repoMock.Verify(r => r.UpdateNotes(It.Is<Note>(n => n.Id == 9 && n.Title == "new" && n.Content == "new" && n.UpdatedAt != default)), Times.Once);
        }

        [Fact]
        public async Task Delete_Post_CallsDeleteAndRedirects()
        {
            _repoMock.Setup(r => r.DeleteNotes(5)).Returns(Task.CompletedTask);

            var result = await _controller.Delete(5);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.Index), redirect.ActionName);
            _repoMock.Verify(r => r.DeleteNotes(5), Times.Once);
        }

        [Fact]
        public async Task UpdateContent_NotFound_WhenNoteMissing()
        {
            _repoMock.Setup(r => r.GetNotesById(42)).ReturnsAsync((Note?)null);
            var dto = new NotesController.InlineUpdateDto { Id = 42, Title = "t", Content = "c" };

            var result = await _controller.UpdateContent(dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateContent_Valid_UpdatesAndReturnsJson()
        {
            var existing = new Note { Id = 11, Title = "a", Content = "b" };
            _repoMock.Setup(r => r.GetNotesById(11)).ReturnsAsync(existing);
            _repoMock.Setup(r => r.UpdateNotes(It.IsAny<Note>())).Returns(Task.CompletedTask);

            var dto = new NotesController.InlineUpdateDto { Id = 11, Title = "newt", Content = "newc" };
            var result = await _controller.UpdateContent(dto);

            var json = Assert.IsType<JsonResult>(result);
            Assert.NotNull(json.Value);

            // verify repository updated and returned value contains expected properties
            _repoMock.Verify(r => r.UpdateNotes(It.Is<Note>(n => n.Id == 11 && n.Title == "newt" && n.Content == "newc" && n.UpdatedAt != default)), Times.Once);

            var value = json.Value;
            var successProp = value.GetType().GetProperty("success");
            var updatedAtProp = value.GetType().GetProperty("updatedAt");

            Assert.NotNull(successProp);
            Assert.NotNull(updatedAtProp);
            Assert.True((bool)successProp.GetValue(value)!);
            var updatedAt = (DateTime)updatedAtProp.GetValue(value)!;
            Assert.True(updatedAt > DateTime.UtcNow.AddMinutes(-1));
        }
    }
}