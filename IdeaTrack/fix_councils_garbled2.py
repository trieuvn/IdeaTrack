# -*- coding: utf-8 -*-
import os

file_path = r'd:\aDesktop\Dotnet\Repo\IdeaTrack\IdeaTrack\Areas\SciTech\Views\Port\Councils.cshtml'

# Read raw bytes and decode as latin-1 to preserve mojibake
with open(file_path, 'rb') as f:
    raw = f.read()
text = raw.decode('latin-1')

# Mapping of garbled Vietnamese to English
replacements = {
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
    # Option values
    'Ch? t?ch': 'Chairman',
    'Thu kÃ¯Â¿Â½': 'Secretary',
    '?y viÃ¯Â¿Â½n': 'Member',
    # Additional garbled strings observed
    'ThÃ¯Â¿Â½nh viÃ¯Â¿Â½n': 'Member',
    'ThÃ¯Â¿Â½m TV': 'Add Member',
    'XÃ¯Â¿Â½a HÃ¯Â¿Â½': 'Delete Board',
    'Nam tÃ¯Â¿Â½i khÃ¯Â¿Â½a': 'Fiscal Year',
    'ThÃ¯Â¿Â½nh viÃ¯Â¿Â½n': 'Member',
    'ChuyÃ¯Â¿Â½n mÃ¯Â¿Â½n': 'Role',
    'Vai trÃ¯Â¿Â½': 'Role',
    'Thao tÃ¯Â¿Â½c': 'Actions',
    'K?t thÃ¯Â¿Â½c': 'Inactive',
    'ThÃ¯Â¿Â½nh viÃ¯Â¿Â½n': 'Member',
    'ThÃ¯Â¿Â½m TV': 'Add Member',
    'XÃ¯Â¿Â½a HÃ¯Â¿Â½': 'Delete Board',
    'ThÃ¯Â¿Â½nh viÃ¯Â¿Â½n': 'Member',
    'ChuyÃ¯Â¿Â½n mÃ¯Â¿Â½n': 'Role',
    'Vai trÃ¯Â¿Â½': 'Role',
    'Thao tÃ¯Â¿Â½c': 'Actions',
    'K?t thÃ¯Â¿Â½c': 'Inactive',
    'ThÃ¯Â¿Â½m TV': 'Add Member',
    'XÃ¯Â¿Â½a HÃ¯Â¿Â½': 'Delete Board',
    'ThÃ¯Â¿Â½nh viÃ¯Â¿Â½n': 'Member',
    'ChuyÃ¯Â¿Â½n mÃ¯Â¿Â½n': 'Role',
    'Vai trÃ¯Â¿Â½': 'Role',
    'Thao tÃ¯Â¿Â½c': 'Actions',
    'K?t thÃ¯Â¿Â½c': 'Inactive',
    'ThÃ¯Â¿Â½m TV': 'Add Member',
    'XÃ¯Â¿Â½a HÃ¯Â¿Â½': 'Delete Board',
    'ThÃ¯Â¿Â½nh viÃ¯Â¿Â½n': 'Member',
    'ChuyÃ¯Â¿Â½n mÃ¯Â¿Â½n': 'Role',
    'Vai trÃ¯Â¿Â½': 'Role',
    'Thao tÃ¯Â¿Â½c': 'Actions',
    'K?t thÃ¯Â¿Â½c': 'Inactive',
    # Modal titles and button texts
    'ThÃ¯Â¿Â½ng tin H?i d?ng': 'Board Information',
    'T?n H?i d?ng': 'Board Name',
    'M? t?': 'Description',
    'H?y': 'Cancel',
    'Luu d? li?u': 'Save',
    'Th?m TV': 'Add Member',
    'X?a H?': 'Delete Board',
    'Ch?a ch?n h?i d?ng': 'Select a board',
    'Ch?n m?t h?i d?ng t? danh s?ch b?n tr?i d? qu?n l?.': 'Select a board from the list to manage.',
}

for vi, en in replacements.items():
    text = text.replace(vi, en)

# Write back as UTF-8
with open(file_path, 'w', encoding='utf-8') as f:
    f.write(text)

print('Garbled Vietnamese in Councils.cshtml fixed')
