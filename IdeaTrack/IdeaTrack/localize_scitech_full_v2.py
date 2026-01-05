# -*- coding: utf-8 -*-
"""localize_scitech_full_v2.py
Extended script to translate all remaining Vietnamese UI strings in the project to English.
It processes .cshtml, .cs, and .js files, applying a comprehensive terminology dictionary
and ensures UTF-8 with BOM encoding.
"""
import os, codecs, re

BASE_DIR = r'd:\aDesktop\Dotnet\Repo\IdeaTrack\IdeaTrack'
VIEW_DIR = os.path.join(BASE_DIR, 'Areas', 'SciTech', 'Views')
CTRL_DIR = os.path.join(BASE_DIR, 'Areas', 'SciTech', 'Controllers')
JS_DIR = os.path.join(BASE_DIR, 'wwwroot', 'js')  # generic JS folder

# Expanded terminology mapping (Vietnamese -> English)
TERMS = {
    # Core domain terms (already present)
    'Quản lý Đợt sáng kiến': 'Initiative Period Management',
    'Quản lý hội đồng': 'Board Management',
    'Quản lý hội đồng': 'Board Management',
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
    # Identity / Auth strings
    'Đăng nhập': 'Login',
    'Mật khẩu': 'Password',
    'Nhớ mật khẩu': 'Remember me',
    'Quên mật khẩu?': 'Forgot password?',
    'Đăng ký': 'Register',
    'Tên đăng nhập': 'Username',
    'Email': 'Email',
    # Alerts / confirms
    'Bạn có chắc muốn xóa?': 'Are you sure you want to delete?',
    'Không thể xóa hội đồng': 'Cannot delete board',
    'Vui lòng nhập thông tin': 'Please enter information',
    'Thành công': 'Success',
    'Thất bại': 'Failure',
    'Lỗi': 'Error',
    'Thông báo': 'Notification',
    'Cảnh báo': 'Warning',
    # DataTable / grid strings
    'Hiển thị _MENU_ mục': 'Show _MENU_ entries',
    'Không có dữ liệu': 'No data available',
    'Đang tải...': 'Loading...',
    'Tìm kiếm:': 'Search:',
    'Trang _PAGE_ của _PAGES_': 'Page _PAGE_ of _PAGES_',
    # Additional phrases observed in other areas
    'Quản lý người dùng': 'User Management',
    'Quản lý dự án': 'Project Management',
    'Thêm dự án': 'Add Project',
    'Xóa dự án': 'Delete Project',
    'Chi tiết': 'Details',
    'Sửa': 'Edit',
    'Xem': 'View',
    'Thêm hội đồng': 'Add Board',
    'Xóa hội đồng': 'Delete Board',
    'Thêm thành viên': 'Add Member',
    'Xóa thành viên': 'Remove Member',
    'Đánh giá': 'Evaluate',
    'Kết quả': 'Result',
    'Báo cáo': 'Report',
    'Tạo mới': 'Create',
    'Thêm mới': 'Create new',
    'Lưu lại': 'Save',
    'Hủy bỏ': 'Cancel',
    'Đóng': 'Close',
    'Thêm mới': 'Create new',
    'Xác nhận xóa': 'Confirm deletion',
    'Bạn có muốn tiếp tục?': 'Do you want to continue?',
    # Generic placeholders
    'Nhập tên...': 'Enter name...',
    'Nhập mô tả...': 'Enter description...',
    'Nhập email...': 'Enter email...',
    'Nhập mật khẩu...': 'Enter password...',
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

def process_dir(root_dir, extensions):
    for root, _, files in os.walk(root_dir):
        for f in files:
            if any(f.lower().endswith(ext) for ext in extensions):
                replace_in_file(os.path.join(root, f))

def main():
    # Process SciTech Views and Controllers
    process_dir(VIEW_DIR, ['.cshtml'])
    process_dir(CTRL_DIR, ['.cs'])
    # Process generic JS folder (if exists)
    if os.path.isdir(JS_DIR):
        process_dir(JS_DIR, ['.js'])
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
