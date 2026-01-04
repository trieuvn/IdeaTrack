# -*- coding: utf-8 -*-
"""localize_scitech_full.py
Automated script to translate all Vietnamese UI text in the SciTech area (Views, Controllers, JS) to English.
Applies the terminology mapping from the implementation plan and ensures UTF-8 with BOM encoding.
"""
import os, codecs, re, sys

BASE_DIR = r'd:\aDesktop\Dotnet\Repo\IdeaTrack\IdeaTrack'
VIEW_DIR = os.path.join(BASE_DIR, 'Areas', 'SciTech', 'Views')
CTRL_DIR = os.path.join(BASE_DIR, 'Areas', 'SciTech', 'Controllers')
JS_DIR = os.path.join(BASE_DIR, 'wwwroot', 'js', 'SciTech')  # adjust if needed

# Comprehensive terminology mapping (Vietnamese -> English)
TERMS = {
    # Core domain terms
    'Quản lý Đợt sáng kiến': 'Initiative Period Management',
    'Quản lý hội đồng': 'Board Management',
    'Quản lý hội đồng': 'Board Management',
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
    'Tên': 'Name',
    'Mô tả': 'Description',
    'Trạng thái': 'Status',
    'Hành động': 'Actions',
    'Thành viên': 'Member',
    'Vai trò': 'Role',
    'Tìm kiếm...': 'Search...',
    'Tìm tên hội đồng...': 'Search board name...',
    'Năm tài khóa': 'Fiscal Year',
    'Thêm mới': 'Create new',
    'Cập nhật': 'Update',
    'Xác nhận': 'Confirm',
    'Xóa hội đồng': 'Delete Board',
    'Thêm thành viên': 'Add Member',
    # Alerts / confirms
    'Bạn có chắc muốn xóa?': 'Are you sure you want to delete?',
    'Không thể xóa hội đồng': 'Cannot delete board',
    'Vui lòng nhập thông tin': 'Please enter information',
    # Identity / Auth strings (common)
    'Đăng nhập': 'Login',
    'Mật khẩu': 'Password',
    'Nhớ mật khẩu': 'Remember me',
    'Quên mật khẩu?': 'Forgot password?',
    'Đăng ký': 'Register',
    # Admin / Author strings (may appear)
    'Quản lý người dùng': 'User Management',
    'Quản lý dự án': 'Project Management',
    # Additional specific phrases observed in previous scan
    'Qu?n lÃ¯Â¿Â½ H?i d?ng': 'Board Management',
    'Thi?t l?p vÃ¯Â¿Â½ qu?n lÃ¯Â¿Â½ thÃ¯Â¿Â½nh viÃ¯Â¿Â½n cÃ¯Â¿Â½c h?i d?ng dÃ¯Â¿Â½nh giÃ¯Â¿Â½.': 'Configure and manage members of the designated boards.',
    'Danh sÃ¯Â¿Â½ch H?i d?ng': 'Board List',
    'TÃ¯Â¿Â½m tÃ¯Â¿Â½n h?i d?ng...': 'Search board name...',
    'K?t thÃ¯Â¿Â½c': 'Inactive',
    'Nam tÃ¯Â¿Â½i khÃ¯Â¿Â½a': 'Fiscal Year',
    'XÃ¯Â¿Â½a HÃ¯Â¿Â½': 'Delete Board',
    'ThÃ¯Â¿Â½m TV': 'Add Member',
    'ThÃ¯Â¿Â½nh viÃ¯Â¿Â½n': 'Member',
    'ChuyÃ¯Â¿Â½n mÃ¯Â¿Â½n': 'Role',
    'Vai trÃ¯Â¿Â½': 'Role',
    'Thao tÃ¯Â¿Â½c': 'Actions',
    # JavaScript DataTable strings (common)
    'Hiển thị _MENU_ mục': 'Show _MENU_ entries',
    'Không có dữ liệu': 'No data available',
    'Đang tải...': 'Loading...',
    'Tìm kiếm:': 'Search:',
    'Trang _PAGE_ của _PAGES_': 'Page _PAGE_ of _PAGES_',
}

# Regex to detect any remaining Vietnamese characters (Unicode range for accented letters)
VIET_RE = re.compile(r'[\u0100-\uFFFF]')

def replace_in_file(path):
    with open(path, 'rb') as f:
        raw = f.read()
    text = raw.decode('latin-1')
    original = text
    for vi, en in TERMS.items():
        if vi in text:
            text = text.replace(vi, en)
    # Write back as UTF-8 with BOM
    if text != original:
        with codecs.open(path, 'w', encoding='utf-8-sig') as out:
            out.write(text)
        print(f'Fixed: {os.path.relpath(path, BASE_DIR)}')
    else:
        # Ensure BOM even if unchanged
        with codecs.open(path, 'w', encoding='utf-8-sig') as out:
            out.write(text)

def process_directory(root_dir, extensions):
    for root, _, files in os.walk(root_dir):
        for f in files:
            if any(f.lower().endswith(ext) for ext in extensions):
                replace_in_file(os.path.join(root, f))

def main():
    # Views (.cshtml)
    process_directory(VIEW_DIR, ['.cshtml'])
    # Controllers (.cs)
    process_directory(CTRL_DIR, ['.cs'])
    # JavaScript files (if any)
    if os.path.isdir(JS_DIR):
        process_directory(JS_DIR, ['.js'])
    # Report any files still containing Vietnamese characters
    leftovers = []
    for root, _, files in os.walk(BASE_DIR):
        for f in files:
            if f.lower().endswith(('.cshtml', '.cs', '.js')):
                p = os.path.join(root, f)
                try:
                    with open(p, 'r', encoding='utf-8') as ff:
                        data = ff.read()
                except UnicodeDecodeError:
                    continue
                if VIET_RE.search(data):
                    leftovers.append(os.path.relpath(p, BASE_DIR))
    if leftovers:
        print('\nFiles with possible remaining Vietnamese characters:')
        for p in leftovers:
            print(' -', p)
    else:
        print('\nAll files processed, no Vietnamese characters detected.')

if __name__ == '__main__':
    main()
