const uploadBox = document.getElementById("uploadBox");
const fileInput = document.getElementById("fileInput");
const fileList = document.getElementById("fileList");
const fileCount = document.getElementById("fileCount");

let files = [];

uploadBox.addEventListener("dragover", e => {
    e.preventDefault();
    uploadBox.classList.add("dragover");
});

uploadBox.addEventListener("dragleave", () => {
    uploadBox.classList.remove("dragover");
});

uploadBox.addEventListener("drop", e => {
    e.preventDefault();
    uploadBox.classList.remove("dragover");
    handleFiles(e.dataTransfer.files);
});

fileInput.addEventListener("change", e => {
    handleFiles(e.target.files);
});

function handleFiles(selectedFiles) {
    [...selectedFiles].forEach(file => {
        if (!validateFile(file)) return;
        files.push(file);
    });
    renderFiles();
}

function validateFile(file) {
    const allowed = [
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    ];
    if (!allowed.includes(file.type)) {
        alert("Invalid file type");
        return false;
    }
    if (file.size > 10 * 1024 * 1024) {
        alert("File too large (max 10MB)");
        return false;
    }
    return true;
}

function renderFiles() {
    fileList.innerHTML = "";
    files.forEach((file, index) => {
        const iconClass = file.type.includes("pdf")
            ? "pdf"
            : file.type.includes("sheet")
                ? "excel"
                : "";

        const item = document.createElement("div");
        item.className = "document-item";

        item.innerHTML = `
            <div class="file-icon ${iconClass}">
                <i class="bi bi-file-earmark"></i>
            </div>
            <div class="file-info">
                <div class="file-name">${file.name}</div>
                <div class="file-meta">${(file.size / 1024 / 1024).toFixed(2)} MB</div>
            </div>
            <div class="file-actions">
                <i class="bi bi-download"></i>
                <i class="bi bi-trash" onclick="removeFile(${index})"></i>
            </div>
        `;

        fileList.appendChild(item);
    });

    fileCount.textContent = files.length;
}

function removeFile(index) {
    files.splice(index, 1);
    renderFiles();
}
