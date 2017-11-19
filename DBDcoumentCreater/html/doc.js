    window.apex_search = {};
apex_search.init = function () {
    this.rows = document.getElementById('tab').getElementsByTagName('TR');
    this.rows_length = apex_search.rows.length;
    this.rows_text = [];
    for (var i = 0; i < apex_search.rows_length; i++) {
        this.rows_text[i] = (apex_search.rows[i].innerText) ? apex_search.rows[i].innerText.toUpperCase() : apex_search.rows[i].textContent.toUpperCase();
    }
    this.time = false;
}

apex_search.lsearch = function () {
    this.term = document.getElementById('S').value.toUpperCase();
    for (var i = 1, row; row = this.rows[i], row_text = this.rows_text[i]; i++) {
        row.style.display = ((row_text.indexOf(this.term) != -1) || this.term === '') ? '' : 'none';
    }
    this.time = false;
}

apex_search.search = function (e) {
    var keycode;
    if (window.event) { keycode = window.event.keyCode; }
    else if (e) { keycode = e.which; }
    else { return false; }
    if (keycode == 13) {
        apex_search.lsearch();
    }
    else { return false; }
}