# -*- coding: utf-8 -*-
"""clean_bom_garbage.py
Removes recursive BOM header garbage from .cs and .cshtml files.
"""
import os, codecs

BASE_DIR = r'd:\aDesktop\Dotnet\Repo\IdeaTrack\IdeaTrack'

# Known garbage patterns that appear at the start of files
GARBAGE_PATTERNS = [
    '\ufeff', # BOM
    'ï»¿',    # BOM read as latin-1
    'A¯A»A¿', # BOM encoded?
    'Ã¯Â»Â¿', # BOM read as UTF-8, then encoded
    'AƒA¯A‚A»A‚A¿',
    'AƒAƒA‚A¯AƒA‚A‚A»AƒA‚A‚A¿',
    # Add sequences seen in the viewer
    'ï»¿A¯A»A¿AƒA¯A‚A»A‚A¿AƒAƒA‚A¯AƒA‚A‚A»AƒA‚A‚A¿', 
]

def clean_file(path):
    try:
        # Read as utf-8-sig to auto-handle the "real" BOM if present
        with open(path, 'r', encoding='utf-8-sig') as f:
            content = f.read()
    except UnicodeDecodeError:
        try:
            with open(path, 'r', encoding='latin-1') as f:
                content = f.read()
        except:
            print(f"Skipping {path}: cannot read")
            return

    original = content
    
    # Iteratively strip garbage from the start
    changed = True
    while changed:
        changed = False
        # Strip confusing unicode BOMs
        if content.startswith('\ufeff'):
            content = content[1:]
            changed = True
            
        # Strip string-literal BOM garbage
        for pattern in GARBAGE_PATTERNS:
            if content.startswith(pattern):
                content = content[len(pattern):]
                changed = True
        
        # Also clean specific mess seen in Councils.cshtml: 
        # ï»¿A¯A»A¿AƒA¯A‚A»A‚A¿AƒAƒA‚A¯AƒA‚A‚A»AƒA‚A‚A¿@model
        # The 'ï»¿' might have been consumed by utf-8-sig, leaving the rest.
        
    if content != original:
        print(f"Cleaned header: {os.path.relpath(path, BASE_DIR)}")
        # Write back as clean UTF-8 with BOM
        with open(path, 'w', encoding='utf-8-sig') as f:
            f.write(content)

def main():
    target_dirs = [
        # Scan entire project
        BASE_DIR
    ]
    
    for d in target_dirs:
        for root, _, files in os.walk(d):
            if 'obj' in root.split(os.sep) or 'bin' in root.split(os.sep):
                continue
            for f in files:
                if f.endswith('.cshtml') or f.endswith('.cs'):
                    clean_file(os.path.join(root, f))

if __name__ == '__main__':
    main()
