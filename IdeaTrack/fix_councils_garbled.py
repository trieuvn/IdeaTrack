# -*- coding: utf-8 -*-
import os

file_path = r'd:\aDesktop\Dotnet\Repo\IdeaTrack\IdeaTrack\Areas\SciTech\Views\Port\Councils.cshtml'

# Read raw bytes and decode as latin-1 to preserve any mojibake characters
with open(file_path, 'rb') as f:
    raw = f.read()
text = raw.decode('latin-1')

# Mapping of garbled Vietnamese strings (as they appear in the file) to English translations
replacements = {
    'Qu?n lï¿½ H?i d?ng': 'Board Management',
    'Thi?t l?p vï¿½ qu?n lï¿½ thï¿½nh viï¿½n cï¿½c h?i d?ng dï¿½nh giï¿½.': 'Configure and manage members of the designated boards.',
    'Danh sï¿½ch H?i d?ng': 'Board List',
    'Tï¿½m tï¿½n h?i d?ng...': 'Search board name...',
    'Thï¿½nh viï¿½n': 'Member',
    'Chuy?n mï¿½n': 'Role',
    'Vai trï¿½': 'Role',
    'Thao tï¿½c': 'Actions',
    'K?t thï¿½c': 'Inactive',
    'Nam tï¿½i khï¿½a': 'Fiscal Year',
    'Th?ng tin H?i d?ng': 'Board Information',
    'T?n H?i d?ng': 'Board Name',
    'M? t?': 'Description',
    'H?y': 'Cancel',
    'Luu d? li?u': 'Save',
    'Th?m TV': 'Add Member',
    'X?a H?': 'Delete Board',
    'Ch?a ch?n h?i d?ng': 'Select a board',
    'Ch?n m?t h?i d?ng t? danh s?ch b?n tr?i d? qu?n l?.': 'Select a board from the list to manage.',
    'Th?m TV': 'Add Member',
    'Xoa': 'Delete',
    'Th?m': 'Add',
    'C?p nh?t': 'Update',
    'Ch?nh s?a': 'Edit',
    'X?a': 'Delete',
    # Additional placeholders that may appear
    'T?n H?i d?ng': 'Board Name',
    'Nam tï¿½i khï¿½a': 'Fiscal Year',
    'M? t?': 'Description',
    'H?y': 'Cancel',
    'Luu d? li?u': 'Save',
    'Th?m TV': 'Add Member',
    'X?a H?': 'Delete Board',
    'Ch?a ch?n h?i d?ng': 'Select a board',
    'Ch?n m?t h?i d?ng t? danh s?ch b?n tr?i d? qu?n l?.': 'Select a board from the list to manage.',
    # Modal and JS strings (garbled)
    "C?NH B�O: X�a h?i d?ng s? x�a t?t c? th?nh vi?n li?n quan. B?n ch?c ch?n ch??": "CONFIRM: Delete board and all related members. Are you sure?",
    "Kh�ng th? x�a h?i d?ng": "Cannot delete board",
    "G? th?nh vi?n n�y kh?i h?i d?ng?": "Remove this member from the board?",
    "Kh�ng th? th�m th?nh vi?n": "Cannot add member",
    "Nh?p t? kh?a d? t�m ki?m nh?n s?": "Enter a keyword to search users...",
    "Kh�ng t�m th?y nh?n s?": "No users found",
    "Ch?n": "Select",
    "Th?ng tin H?i d?ng": "Board Information",
    "T?n H?i d?ng": "Board Name",
    "Nam t?i kh?a": "Fiscal Year",
    "M? t?": "Description",
    "H?y": "Cancel",
    "Luu d? li?u": "Save",
    "Th?m TV": "Add Member",
    "X?a H?": "Delete Board",
    "Ch?a ch?n h?i d?ng": "Select a board",
    "Ch?n m?t h?i d?ng t? danh s?ch b?n tr?i d? qu?n l?.": "Select a board from the list to manage.",
    "Th?m TV": "Add Member",
    "Xoa": "Delete",
    "Th?m": "Add",
    "C?p nh?t": "Update",
    "Ch?nh s?a": "Edit",
    "X?a": "Delete",
    "Th?m TV": "Add Member",
}

for vi, en in replacements.items():
    text = text.replace(vi, en)

# Write back as UTF-8
with open(file_path, 'w', encoding='utf-8') as f:
    f.write(text)

print('Fixed garbled Vietnamese in Councils.cshtml')
