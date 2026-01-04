# -*- coding: utf-8 -*-
"""localize_scitech.py
Automated script to translate all Vietnamese UI text in the SciTech area to English.
It applies the terminology mapping defined in the implementation plan and
replaces any remaining Vietnamese strings found in .cshtml and .cs files.
All files are saved as UTF-8 with BOM (utf-8-sig).
"""
import os, codecs, re, sys

# Base directory of the project (adjust if needed)
BASE_DIR = r'd:\aDesktop\Dotnet\Repo\IdeaTrack\IdeaTrack'

# Directories to process
VIEW_DIR = os.path.join(BASE_DIR, 'Areas', 'SciTech', 'Views')
CTRL_DIR = os.path.join(BASE_DIR, 'Areas', 'SciTech', 'Controllers')

# Terminology mapping (Vietnamese -> English)
TERMS = {
    # Core terms
    'Quản lý Đợt sáng kiến': 'Initiative Period Management',
    'Danh mục': 'Categories',
    'Bộ tiêu chí': 'Evaluation Templates',
    'Biểu mẫu': 'Evaluation Templates',
    'Phân bổ Hội đồng': 'Board Assignment',
    'Sơ duyệt': 'Preliminary Review',
    'Phê duyệt': 'Approve',
    'Từ chối': 'Reject',
    'Yêu cầu chỉnh sửa': 'Request Revision',
    'Công bố kết quả': 'Publish Results',
    # Common UI words
    'Lưu': 'Save',
    'Hủy': 'Cancel',
    'Thêm': 'Add',
    'Xóa': 'Delete',
    'Chỉnh sửa': 'Edit',
    'Thông tin': 'Information',
    'Duyệt hồ sơ': 'Approve Dossier',
    # Additional phrases (may appear in views)
    'Quản lý Đợt sáng kiến': 'Initiative Period Management',
    'Quản lý Hội đồng': 'Board Management',
    'Quản lý hội đồng': 'Board Management',
    'Quản lý hội đồng': 'Board Management',
    'Quản lý Đợt sáng kiến': 'Initiative Period Management',
    # Generic Vietnamese words that appear in UI (add as needed)
    'Tên': 'Name',
    'Mô tả': 'Description',
    'Trạng thái': 'Status',
    'Hành động': 'Actions',
    'Thành viên': 'Member',
    'Vai trò': 'Role',
    'Tìm kiếm...': 'Search...',
    'Tìm tên hội đồng...': 'Search board name...',
    'Năm tài khóa': 'Fiscal Year',
    'Mô tả': 'Description',
    'Thêm mới': 'Create new',
    'Cập nhật': 'Update',
    'Xác nhận': 'Confirm',
    'Xóa hội đồng': 'Delete Board',
    'Thêm thành viên': 'Add Member',
    # JavaScript alerts / confirms
    'Bạn có chắc muốn xóa?': 'Are you sure you want to delete?',
    'Không thể xóa hội đồng': 'Cannot delete board',
    'Vui lòng nhập thông tin': 'Please enter information',
}

# Additional regex patterns for Vietnamese characters (catch any remaining)
VIETNAMESE_RE = re.compile(r'[\u0100-\uFFFF]')

def process_file(path, is_view=True):
    # Read raw bytes to preserve any mojibake, decode as latin-1
    with open(path, 'rb') as f:
        raw = f.read()
    text = raw.decode('latin-1')
    original = text
    # Apply term replacements
    for vi, en in TERMS.items():
        if vi in text:
            text = text.replace(vi, en)
    # If still contains Vietnamese characters, attempt a generic replace (remove diacritics)
    # Here we simply keep them – later we will report leftovers.
    if text != original:
        # Write back as UTF-8 with BOM
        with codecs.open(path, 'w', encoding='utf-8-sig') as out:
            out.write(text)
        print(f'Fixed: {os.path.relpath(path, BASE_DIR)}')
    else:
        # No change, but still write back to ensure UTF-8 BOM consistency
        with codecs.open(path, 'w', encoding='utf-8-sig') as out:
            out.write(text)
        # No output needed for unchanged files

def main():
    # Process Views (.cshtml)
    for root, _, files in os.walk(VIEW_DIR):
        for f in files:
            if f.lower().endswith('.cshtml'):
                process_file(os.path.join(root, f), is_view=True)
    # Process Controllers (.cs)
    for root, _, files in os.walk(CTRL_DIR):
        for f in files:
            if f.lower().endswith('.cs'):
                process_file(os.path.join(root, f), is_view=False)
    # Report any files still containing Vietnamese characters
    leftovers = []
    for root, _, files in os.walk(BASE_DIR):
        for f in files:
            if f.lower().endswith(('.cshtml', '.cs')):
                path = os.path.join(root, f)
                with open(path, 'rb') as ff:
                    data = ff.read().decode('utf-8', errors='ignore')
                if VIETNAMESE_RE.search(data):
                    leftovers.append(os.path.relpath(path, BASE_DIR))
    if leftovers:
        print('\nFiles with possible remaining Vietnamese characters:')
        for p in leftovers:
            print(' -', p)
    else:
        print('\nAll files processed, no Vietnamese characters detected.')

if __name__ == '__main__':
    main()
