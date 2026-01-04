# -*- coding: utf-8 -*-
import os, re

file_path = r'd:\aDesktop\Dotnet\Repo\IdeaTrack\IdeaTrack\Areas\SciTech\Views\Port\Councils.cshtml'

# Load content as latin-1 to preserve raw bytes, then decode to str
with open(file_path, 'rb') as f:
    raw = f.read()
text = raw.decode('latin-1')

# Mapping of Vietnamese phrases to English
replacements = {
    'Qu?n l� H?i d?ng': 'Board Management',
    'Thi?t l?p v� qu?n l� th�nh vi�n c�c h?i d?ng d�nh gi�.': 'Configure and manage members of the designated boards.',
    'Danh s�ch H?i d?ng': 'Board List',
    'Create new': 'Create new',  # keep as is maybe
    'T�m t�n h?i d?ng...': 'Search board name...',
    'Th�nh vi�n': 'Member',
    'Chuy�n m�n': 'Role',
    'Vai tr�': 'Role',
    'Thao t�c': 'Actions',
    'Ho?t d?ng': 'Active',
    'K?t th�c': 'Inactive',
    'Nam t�i kh�a': 'Fiscal Year',
    'Th�ng tin H?i d?ng': 'Board Information',
    'T�n H?i d?ng': 'Board Name',
    'Nam t�i kh�a': 'Fiscal Year',
    'M� t?': 'Description',
    'H?y': 'Cancel',
    'Luu d? li?u': 'Save',
    'Th�m TV': 'Add Member',
    'X�a H�': 'Delete Board',
    'Ch?a ch?n h?i d?ng': 'Select a board',
    'Ch?n m?t h?i d?ng t? danh s�ch b�n tr�i d? qu?n l�.': 'Select a board from the list to manage.',
    'Th�ng tin H?i d?ng': 'Board Information',
    'Th�ng tin H?i d?ng': 'Board Information',
    'Xoa': 'Delete',
    'Thêm': 'Add',
    'C?p nh?t': 'Update',
    'Ch?nh s?a': 'Edit',
    'X?a': 'Delete',
    'Th?m TV': 'Add Member',
    # Modal and JS strings
    "C?NH B�O: X�a h?i d?ng s? x�a t?t c? th�nh vi�n li�n quan. B?n ch?c ch?n ch??": "CONFIRM: Delete board and all related members. Are you sure?",
    "Kh�ng th? x�a h?i d?ng": "Cannot delete board",
    "G? th�nh vi�n n�y kh?i h?i d?ng?": "Remove this member from the board?",
    "Kh�ng th? th�m th�nh vi�n": "Cannot add member",
    "Nh?p t? kh�a d? t�m ki?m nh�n s?": "Enter a keyword to search users...",
    "Kh�ng t�m th?y nh�n s?": "No users found",
    "Ch?n": "Select",
    "Th�ng tin H?i d?ng": "Board Information",
    "T�n H?i d?ng": "Board Name",
    "Nam t�i kh�a": "Fiscal Year",
    "M� t?": "Description",
    "H?y": "Cancel",
    "Luu d? li?u": "Save",
    "Th�m TV": "Add Member",
    "X�a H�": "Delete Board",
    "Ch?a ch?n h?i d?ng": "Select a board",
    "Ch?n m?t h?i d?ng t? danh s�ch b�n tr�i d? qu?n l�.": "Select a board from the list to manage.",
    "Th?m TV": "Add Member",
    "Xoa": "Delete",
    "Th?m": "Add",
    "C?p nh?t": "Update",
    "Ch?nh s?a": "Edit",
    "X?a": "Delete",
    "Th?m TV": "Add Member",
    # Additional placeholders
    "T�n H?i d?ng": "Board Name",
    "Nam t�i kh�a": "Fiscal Year",
    "M� t?": "Description",
    "H?y": "Cancel",
    "Luu d? li?u": "Save",
    "Th�m TV": "Add Member",
    "X�a H�": "Delete Board",
    "Ch?a ch?n h?i d?ng": "Select a board",
    "Ch?n m?t h?i d?ng t? danh s�ch b�n tr�i d? qu?n l�.": "Select a board from the list to manage.",
    "Th?m TV": "Add Member",
    "Xoa": "Delete",
    "Th?m": "Add",
    "C?p nh?t": "Update",
    "Ch?nh s?a": "Edit",
    "X?a": "Delete",
    "Th?m TV": "Add Member",
    # Ensure no duplicate keys
}

# Apply replacements
for vi, en in replacements.items():
    text = text.replace(vi, en)

# Write back as UTF-8
with open(file_path, 'w', encoding='utf-8') as f:
    f.write(text)

print('Fixed Councils.cshtml')
