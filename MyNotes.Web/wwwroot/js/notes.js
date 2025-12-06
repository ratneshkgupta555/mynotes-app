// Inline edit + AJAX using Fetch and antiforgery token from the page forms
(function () {
    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }

    document.addEventListener('click', function (e) {
        if (e.target.matches('.btn-edit')) {
            const card = e.target.closest('.card');
            card.querySelector('.inline-editor').style.display = 'block';
            card.querySelector('.note-title').style.display = 'none';
            card.querySelector('.note-content').style.display = 'none';
            e.target.style.display = 'none';
        }

        if (e.target.matches('.btn-cancel-inline')) {
            const card = e.target.closest('.card');
            card.querySelector('.inline-editor').style.display = 'none';
            card.querySelector('.note-title').style.display = '';
            card.querySelector('.note-content').style.display = '';
            card.querySelector('.btn-edit').style.display = '';
        }

        if (e.target.matches('.btn-save-inline')) {
            const card = e.target.closest('.card');
            const id = parseInt(card.getAttribute('data-id'), 10);
            const title = card.querySelector('.inline-title').value;
            const content = card.querySelector('.inline-content').value;
            const token = getAntiForgeryToken();

            fetch('/Notes/UpdateContent', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ id: id, title: title, content: content })
            }).then(r => {
                if (!r.ok) throw new Error('Network response not ok');
                return r.json();
            }).then(() => {
                card.querySelector('.note-title').textContent = title;
                card.querySelector('.note-content').innerHTML = content.replace(/\n/g, '<br />');
                card.querySelector('.inline-editor').style.display = 'none';
                card.querySelector('.note-title').style.display = '';
                card.querySelector('.note-content').style.display = '';
                card.querySelector('.btn-edit').style.display = '';
            }).catch(err => alert('Could not save inline edit: ' + err));
        }
    });
})();