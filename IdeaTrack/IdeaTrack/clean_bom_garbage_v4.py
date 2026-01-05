# -*- coding: utf-8 -*-
"""clean_bom_garbage_v4.py
Strips all content before the first valid code keyword.
"""
import os

BASE_DIR = r'd:\aDesktop\Dotnet\Repo\IdeaTrack\IdeaTrack'

VALID_STARTS = [
    'using ',
    'namespace ',
    '@model',
    '@page',
    '@using',
    '@addTagHelper',
    '@inherits',
    '@{',
    '<!DOCTYPE',
    '<html',
    '<head',
    '<body',
    '//',
    '/*',
    'package ',
    '<!--',
    '<'
]

def clean_file(path):
    try:
        # Read as utf-8-sig
        with open(path, 'r', encoding='utf-8-sig') as f:
            content = f.read()
    except UnicodeDecodeError:
        try:
            with open(path, 'r', encoding='latin-1') as f:
                content = f.read()
        except:
            return

    original = content
    
    # Find the earliest occurrence of any valid start token
    min_idx = -1
    
    for token in VALID_STARTS:
        idx = content.find(token)
        if idx != -1:
            if min_idx == -1 or idx < min_idx:
                min_idx = idx
                
    if min_idx > 0:
        # Check if the prefix contains only whitespace. If so, don't touch it.
        prefix = content[:min_idx]
        if not prefix.strip():
            pass # It's just whitespace (or BOMs that python handled), ignore
        else:
            # It's garbage!
            # Strip it.
            print(f"Stripping {min_idx} chars of header garbage from {os.path.relpath(path, BASE_DIR)}")
            content = content[min_idx:]
            
            with open(path, 'w', encoding='utf-8-sig') as f:
                f.write(content)

def main():
    target_dirs = [BASE_DIR]
    
    for d in target_dirs:
        for root, _, files in os.walk(d):
            if 'obj' in root.split(os.sep) or 'bin' in root.split(os.sep):
                continue
            for f in files:
                if f.endswith('.cshtml') or f.endswith('.cs'):
                    clean_file(os.path.join(root, f))

if __name__ == '__main__':
    main()
