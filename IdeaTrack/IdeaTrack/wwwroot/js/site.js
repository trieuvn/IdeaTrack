﻿// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// DataTables Initialization
$(document).ready(function () {
    if ($('#basic-datatables').length) {
        var table = $('#basic-datatables').DataTable({
            "pageLength": 10,
            "lengthMenu": [[5, 10, 25, 50, -1], [5, 10, 25, 50, "All"]],
            "language": {
                "search": "Search:",
                "lengthMenu": "Show _MENU_ entries",
                "info": "Showing _START_ to _END_ of _TOTAL_ entries",
                "infoEmpty": "No entries available",
                "infoFiltered": "(filtered from _MAX_ total entries)",
                "paginate": {
                    "first": "First",
                    "last": "Last",
                    "next": "Next",
                    "previous": "Previous"
                },
                "emptyTable": "No data available in table"
            },
            "order": [[2, "desc"]],
            "columnDefs": [
                { "orderable": false, "targets": 4 }
            ]
        });

        // Filter tabs functionality - connect with DataTables
        $('[data-filter]').on('click', function () {
            // Update active state
            $('[data-filter]').removeClass('active');
            $(this).addClass('active');

            var filterValue = $(this).data('filter');

            if (filterValue === 'all') {
                // Show all records
                table.column(3).search('').draw();
            } else {
                // Filter by status column (index 3)
                table.column(3).search(filterValue, false, true).draw();
            }
        });
    }
});

// File upload handler
document.addEventListener("DOMContentLoaded", function () {
    const fileInput = document.getElementById("fileInput");
    const fileList = document.getElementById("fileList");
    const uploadHint = document.getElementById("uploadHint");

    if (!fileInput) return;

    fileInput.addEventListener("change", function () {
        if (this.files.length === 0) return;
        if (uploadHint) uploadHint.style.display = "none";
        if (fileList) {
            fileList.innerHTML = "";
            Array.from(this.files).forEach(file => {
                const div = document.createElement("div");
                div.className = "file";
                div.innerHTML = `<i class="bi bi-file-earmark"></i><span>${file.name}</span>`;
                fileList.appendChild(div);
            });
        }
    });
});

// Filter tabs
document.addEventListener("DOMContentLoaded", function () {
    const tabs = document.querySelectorAll('.filter-tabs .tab');
    const cards = document.querySelectorAll('.history-card');
    if (tabs.length > 0 && cards.length > 0) {
        tabs.forEach(tab => {
            tab.addEventListener('click', () => {
                tabs.forEach(t => t.classList.remove('active'));
                tab.classList.add('active');
                const status = tab.textContent.trim().toLowerCase();
                cards.forEach(card => {
                    card.style.display = status === 'all status' || card.classList.contains(status) ? 'flex' : 'none';
                });
            });
        });
    }
});
