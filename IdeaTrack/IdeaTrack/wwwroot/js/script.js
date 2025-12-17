const buttons = document.querySelectorAll(".source-btn");
const existingFiles = document.getElementById("existingFiles");

const localUploadBox = document.getElementById("uploadBox");
const localFileInput = document.getElementById("fileInput");
const localFileList = document.getElementById("fileList");
let files = [];

/* ===== TOGGLE FILE SOURCE ===== */
buttons.forEach(btn => {
    btn.addEventListener("click", () => {
        buttons.forEach(b => b.classList.remove("active"));
        btn.classList.add("active");

        if (btn.dataset.source === "existing") {
            existingFiles.style.display = "block";
            localUploadBox.style.display = "none";
        } else {
            existingFiles.style.display = "none";
            localUploadBox.style.display = "block";
        }
    });
});

/* ===== CLICK UPLOAD BOX ===== */
localUploadBox.addEventListener("click", () => {
    localFileInput.click();
});

/* ===== DRAG & DROP ===== */
localUploadBox.addEventListener("dragover", e => {
    e.preventDefault();
    localUploadBox.classList.add("dragover");
});
localUploadBox.addEventListener("dragleave", () => {
    localUploadBox.classList.remove("dragover");
});
localUploadBox.addEventListener("drop", e => {
    e.preventDefault();
    localUploadBox.classList.remove("dragover");
    handleFiles(e.dataTransfer.files);
});

/* ===== FILE INPUT ===== */
localFileInput.addEventListener("change", e => {
    handleFiles(e.target.files);
});

/* ===== HANDLE FILES ===== */
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
    localFileList.innerHTML = "";
    files.forEach((file, index) => {
        const item = document.createElement("div");
        item.className = "document-item";
        item.innerHTML = `
            <div class="file-icon">
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
        localFileList.appendChild(item);
    });
}

function removeFile(index) {
    files.splice(index, 1);
    renderFiles();
}


function toggleSelect(el) {
    el.classList.toggle("selected");
}
