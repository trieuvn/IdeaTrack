# -*- coding: utf-8 -*-
import os, glob

# Directory containing the Port views
base_dir = r'd:\\aDesktop\\Dotnet\\Repo\\IdeaTrack\\IdeaTrack\\Areas\\SciTech\\Views\\Port'

# Gather all .cshtml files
files = glob.glob(os.path.join(base_dir, '*.cshtml'))

# Mapping of garbled Vietnamese (as appears in files) to English
replacements = {
    # Common garbled patterns
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
    'ThÃ¯Â¿Â½nh viÃ¯Â¿Â½n': 'Member',
    'ThÃ¯Â¿Â½m TV': 'Add Member',
    'XÃ¯Â¿Â½a HÃ¯Â¿Â½': 'Delete Board',
    'ThÃ¯Â¿Â½nh viÃ¯Â¿Â½n': 'Member',
    'ChuyÃ¯Â¿Â½n mÃ¯Â¿Â½n': 'Role',
    'Vai trÃ¯Â¿Â½': 'Role',
    'Thao tÃ¯Â¿Â½c': 'Actions',
    'K?t thÃ¯Â¿Â½c': 'Inactive',
    # Option values
    'Chairman': 'Chairman',
    'Thu kÃ¯Â¿Â½': 'Secretary',
    '?y viÃ¯Â¿Â½n': 'Member',
    # Modal and button texts
    'ThÃ¯Â¿Â½ng tin H?i d?ng': 'Board Information',
    'T?n H?i d?ng': 'Board Name',
    'M? t?': 'Description',
    'H?y': 'Cancel',
    'Luu d? li?u': 'Save',
    'Th?m TV': 'Add Member',
    'X?a H?': 'Delete Board',
    'Ch?a ch?n h?i d?ng': 'Select a board',
    'Ch?n m?t h?i d?ng t? danh s?ch b?n tr?i d? qu?n l?.': 'Select a board from the list to manage.',
    # Follow view specific
    'Gi?ng viên nh?n': 'Assigned Lecturer',
    'Ch?n h? so d? hi?n th?': 'Select a file to display',
    'N?i dung y?u c?u ch?nh s?a': 'Revision Request Content',
    'Nh?p chi ti?t c?c m?c c?n ch?nh s?a...': 'Enter details of items to be revised...',
    'B?t bu?c tru?c khi chuy?n h? so l?n PKHCN': 'Required before forwarding to Science Department',
    'H?n n?p l?i': 'Resubmission Deadline',
    'G?i y?u c?u ch?nh s?a': 'Send Revision Request',
    '\u00d0ang ch?nh s?a': 'Under Revision',
    '\u00d0\u00e3 n?p l?i': 'Resubmitted',
    'Ti?n d? h? so': 'File Progress',
    'T?t c?': 'All',
    'Ch?nh s?a': 'Revisions',
    'N?p l?i': 'Resubmit',
    '\u00d0? t\u00e0i:': 'Project:',
    'T?i uu h\u00f3a nang lu?ng t\u00e1i t?o': 'Renewable Energy Optimization',
    'H? so:': 'File:',
    '\u00d0ang k\u00fd s\u00e1ng ki?n c?p Faculty': 'Faculty-level Initiative Registration',
    'Xem chi ti?t': 'View Details',
    'Ghi ch\u00fa t? GV:': 'Notes from Lecturer:',
    '\u00d0\u00e3 b? sung m?c 3.2 theo y\u00eau c?u c?a Faculty.': 'Added section 3.2 as requested by Faculty.',
    'Y\u00ea?u c?u:': 'Request:',
    'Thi?u minh ch?ng ?nh hu?ng c?a s\u00e1ng ki?n nam 2023.': 'Missing evidence of initiative impact in 2023.',
    'Nh?c nh?': 'Remind',
    'Hi?n th?': 'Showing',
    'h? so': 'files',
    'gi? tru?c': 'hours ago',
    'ng\u00e0y tru?c': 'days ago',
    'GV: Tr?n Minh Tu?n': 'Lecturer: Tran Minh Tuan',
    'GV: Nguy?n Th? Lan': 'Lecturer: Nguyen Thi Lan',
    'NLP ?ng d?ng': 'NLP Application',
    'GV. Tr?n Th? B': 'Lecturer Tran Thi B',
    'GV. L\u00ea Van C': 'Lecturer Le Van C',
    # Result view specific
    'K?t qu? chung cu?c': 'Final Result',
    '\u00d0?t y\u00eau c?u': 'Passed',
    '\u00d0? ngh? ti?p t?c th?c hi?n': 'Recommended to Continue',
    'T? l? d?ng thu?n': 'Consensus Rate',
    'Chi ti?t di?m th\u00e0nh vi\u00ean': 'Member Score Details',
    'Xem bi?u d?': 'View Chart',
    'Th\u00e0nh vi\u00ean': 'Member',
    'Vai tr\u00f2': 'Role',
    'T?ng': 'Total',
    'Phi?u': 'Vote',
    'Ch? t?ch': 'Chairman',
    'Ph?n bi?n 1': 'Reviewer 1',
    'Ph?n bi?n 2': 'Reviewer 2',
    'T?ng h?p \u00fd ki?n': 'Consolidated Feedback',
    'Uu di?m': 'Strengths',
    'H?n ch?': 'Limitations',
    'Ki?n ngh?': 'Recommendations',
    'Chua r\u00f5 phuong \u00e1n thuong m?i h\u00f3a': 'Commercialization plan unclear',
    'D? to\u00e1n ph?n m?m c\u00f2n cao': 'Software budget still high',
    'B? sung d?a di?m th? nghi?m': 'Add trial location',
    'gi?m 15% chi ph\u00ed chuy\u00ean gia nu?c ngo\u00e0i.': 'reduce foreign expert costs by 15%.',
    '\u00d0H B\u00e1ch Khoa': 'Polytechnic University',
    'Vi?n N\u00f4ng nghi?p': 'Agriculture Institute',
    'S? KHCN': 'Dept. of Science & Tech',
    'Tr?n Van B\u00ecnh': 'Tran Van Binh',
    'L\u00ea Ho\u00e0ng Lan': 'Le Hoang Lan',
    'Nguy?n Minh': 'Nguyen Minh',
    # Rule view specific
    'Qu?n l\u00fd Ti\u00eau ch\u00ed & Tr?ng s?': 'Criteria & Weight Management',
    'T?ng tr?ng s? c?a t?t c? ti\u00eau ch\u00ed ph?i b?ng': 'Total weight of all criteria must equal',
    'd? k\u00edch ho?t ch?m di?m.': 'to activate grading.',
    'T?ng s? ti\u00eau ch\u00ed': 'Total Criteria',
    'T?ng tr?ng s?': 'Total Weight',
    'H?p l?': 'Valid',
    'C?p nh?t l?n cu?i': 'Last Updated',
    'b?i Admin': 'by Admin',
    'Danh s\u00e1ch ti\u00eau ch\u00ed': 'Criteria List',
    'Th\u00eam ti\u00eau ch\u00ed': 'Add Criteria',
    'T\u00ean ti\u00eau ch\u00ed': 'Criteria Name',
    'M\u00f4 t?': 'Description',
    'Tr?ng s? (%)': 'Weight (%)',
    'Thao t\u00e1c': 'Actions',
    # Template view specific
    'Ch?m ?i?m': 'Scoring',
    'Duy?t/T?i?c': 'Approve/Reject',
}

for file_path in files:
    with open(file_path, 'rb') as f:
        raw = f.read()
    text = raw.decode('latin-1')
    original = text
    for vi, en in replacements.items():
        if vi in text:
            text = text.replace(vi, en)
    if text != original:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(text)
        print(f'Fixed: {os.path.basename(file_path)}')
    else:
        print(f'No changes needed: {os.path.basename(file_path)}')
print('All Port view files processed')
