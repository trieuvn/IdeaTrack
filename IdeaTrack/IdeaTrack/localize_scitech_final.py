# -*- coding: utf-8 -*-
"""localize_scitech_final.py
This script performs a final pass over all .cshtml, .cs, and .js files in the project.
It:
1. Applies the existing TERM replacements (same as in localize_scitech_full_v2.py).
2. Transliterate any remaining Vietnamese characters with diacritics to their ASCII equivalents
   (e.g., "á" → "a", "đ" → "d"). This ensures no non‑ASCII characters remain.
3. Saves files as UTF‑8 with BOM.
"""
import os, codecs, re

BASE_DIR = r'd:\aDesktop\Dotnet\Repo\IdeaTrack\IdeaTrack'
VIEW_DIR = os.path.join(BASE_DIR, 'Areas', 'SciTech', 'Views')
CTRL_DIR = os.path.join(BASE_DIR, 'Areas', 'SciTech', 'Controllers')
JS_DIR = os.path.join(BASE_DIR, 'wwwroot', 'js')

# Base term replacements (same as before, can be extended)
TERMS = {
    'Quản lý Đợt sáng kiến': 'Initiative Period Management',
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
    'Năm tài khóa': 'Fiscal Year',
    'Đăng nhập': 'Login',
    'Mật khẩu': 'Password',
    'Nhớ mật khẩu': 'Remember me',
    'Quên mật khẩu?': 'Forgot password?',
    'Đăng ký': 'Register',
    'Bạn có chắc muốn xóa?': 'Are you sure you want to delete?',
    'Không thể xóa hội đồng': 'Cannot delete board',
    'Vui lòng nhập thông tin': 'Please enter information',
    'Thành công': 'Success',
    'Thất bại': 'Failure',
    'Lỗi': 'Error',
    'Thông báo': 'Notification',
    'Cảnh báo': 'Warning',
    'Hiển thị _MENU_ mục': 'Show _MENU_ entries',
    'Không có dữ liệu': 'No data available',
    'Đang tải...': 'Loading...',
    'Tìm kiếm:': 'Search:',
    'Trang _PAGE_ của _PAGES_': 'Page _PAGE_ of _PAGES_',
}

# Mapping of Vietnamese diacritics to ASCII equivalents
VIET_MAP = {
    'á':'a','à':'a','ả':'a','ã':'a','ạ':'a','ă':'a','ắ':'a','ằ':'a','ẳ':'a','ẵ':'a','ặ':'a','â':'a','ấ':'a','ầ':'a','ẩ':'a','ẫ':'a','ậ':'a',
    'Á':'A','À':'A','Ả':'A','Ã':'A','Ạ':'A','Ă':'A','Ắ':'A','Ằ':'A','Ẳ':'A','Ẵ':'A','Ặ':'A','Â':'A','Ấ':'A','Ầ':'A','Ẩ':'A','Ẫ':'A','Ậ':'A',
    'é':'e','è':'e','ẻ':'e','ẽ':'e','ẹ':'e','ê':'e','ế':'e','ề':'e','ể':'e','ễ':'e','ệ':'e',
    'É':'E','È':'E','Ẻ':'E','Ẽ':'E','Ẹ':'E','Ê':'E','Ế':'E','Ề':'E','Ể':'E','Ễ':'E','Ệ':'E',
    'í':'i','ì':'i','ỉ':'i','ĩ':'i','ị':'i','Í':'I','Ì':'I','Ỉ':'I','Ĩ':'I','Ị':'I',
    'ó':'o','ò':'o','ỏ':'o','õ':'o','ọ':'o','ô':'o','ố':'o','ồ':'o','ổ':'o','ỗ':'o','ộ':'o','ơ':'o','ớ':'o','ờ':'o','ở':'o','ỡ':'o','ợ':'o',
    'Ó':'O','Ò':'O','Ỏ':'O','Õ':'O','Ọ':'O','Ô':'O','Ố':'O','Ồ':'O','Ổ':'O','Ỗ':'O','Ộ':'O','Ơ':'O','Ớ':'O','Ờ':'O','Ở':'O','Ỡ':'O','Ợ':'O',
    'ú':'u','ù':'u','ủ':'u','ũ':'u','ụ':'u','ư':'u','ứ':'u','ừ':'u','ử':'u','ữ':'u','ự':'u',
    'Ú':'U','Ù':'U','Ủ':'U','Ũ':'U','Ụ':'U','Ư':'U','Ứ':'U','Ừ':'U','Ử':'U','Ữ':'U','Ự':'U',
    'ý':'y','ỳ':'y','ỷ':'y','ỹ':'y','ỵ':'y','Ý':'Y','Ỳ':'Y','Ỷ':'Y','Ỹ':'Y','Ỵ':'Y',
    'đ':'d','Đ':'D'
}

VIET_RE = re.compile(r'[\u0100-\uFFFF]')

def transliterate(text):
    return ''.join(VIET_MAP.get(ch, ch) for ch in text)

def replace_in_file(path):
    with open(path, 'rb') as f:
        raw = f.read()
    txt = raw.decode('latin-1')
    original = txt
    # Apply term replacements
    for vi, en in TERMS.items():
        if vi in txt:
            txt = txt.replace(vi, en)
    # Transliterate any remaining Vietnamese characters
    if VIET_RE.search(txt):
        txt = transliterate(txt)
    # Write back as UTF-8 with BOM
    if txt != original:
        with codecs.open(path, 'w', encoding='utf-8-sig') as out:
            out.write(txt)
        print(f'Fixed: {os.path.relpath(path, BASE_DIR)}')
    else:
        # Ensure BOM even if unchanged
        with codecs.open(path, 'w', encoding='utf-8-sig') as out:
            out.write(txt)

def process_dir(root_dir, exts):
    for root, _, files in os.walk(root_dir):
        for f in files:
            if any(f.lower().endswith(ext) for ext in exts):
                replace_in_file(os.path.join(root, f))

def main():
    process_dir(VIEW_DIR, ['.cshtml'])
    process_dir(CTRL_DIR, ['.cs'])
    if os.path.isdir(JS_DIR):
        process_dir(JS_DIR, ['.js'])
    # Final check for any remaining Vietnamese characters
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
        print('\nFiles still containing Vietnamese characters:')
        for p in leftovers:
            print(' -', p)
    else:
        print('\nAll files processed, no Vietnamese characters remain.')

if __name__ == '__main__':
    main()
