# -*- coding: utf-8 -*-
"""clean_bom_garbage_v3.py
Aggressively removes non-ASCII header garbage from .cs and .cshtml files.
"""
import os

BASE_DIR = r'd:\aDesktop\Dotnet\Repo\IdeaTrack\IdeaTrack'

def clean_file(path):
    try:
        # Read as utf-8-sig
        with open(path, 'r', encoding='utf-8-sig') as f:
            content = f.read()
    except UnicodeDecodeError:
        try:
            # Fallback to latin-1 to just get the bytes
            with open(path, 'r', encoding='latin-1') as f:
                content = f.read()
        except:
            print(f"Skipping {path}: cannot read")
            return

    original = content
    
    # Strip all non-ASCII characters from the BEGINNING of the file
    # Stop at the first ASCII character (that is not whitespace maybe? No, let's keep whitespace)
    # Actually, we just want to remove the garbage prefix.
    # The garbage is likely a sequence of non-ASCII chars.
    
    new_content = content
    stripped_count = 0
    
    while len(new_content) > 0 and ord(new_content[0]) > 127:
        new_content = new_content[1:]
        stripped_count += 1
        
    # Also strip potentially leading 'A' if it's followed by garbage that we just stripped? 
    # The previous pattern was Aƒ... If we strip Aƒ, we strip A (valid) and ƒ (invalid).
    # Wait, 'A' is valid ASCII.
    # If the file starts with "Aƒ...", then 'A' is ASCII. failing to strip it might leave "Ausing System;".
    # We should look for specific known garbage prefixes even if they contain ASCII.
    # The garbage usually starts with BOM components.
    
    # Let's rely on the fact that C# files start with 'using', 'namespace', '//', 'public', etc.
    # And Views start with '@', '<', etc.
    # If the file starts with 'A' followed by non-ASCII, it's garbage.
    
    if len(new_content) > 0 and stripped_count == 0:
        # Check for the specific "Aƒ..." or "ï»¿" patterns again using raw check
        # because unicode values might be deceiving.
        pass

    # Specific check for the 'A' prefix if followed by non-ASCII (which we might have failed to read if decoded as ASCII?)
    # If we read as UTF-8, 'Aƒ' is 'A' (U+0041) and 'ƒ' (U+0192).
    # 'ƒ' is > 127. So 'A' would remain.
    # We should check if the first char is 'A' and the second is > 127.
    if len(new_content) > 1 and new_content[0] == 'A' and ord(new_content[1]) > 127:
        new_content = new_content[1:] # Strip the 'A'
        # Then strip the rest
        while len(new_content) > 0 and ord(new_content[0]) > 127:
            new_content = new_content[1:]
            
    if new_content != original:
        print(f"Aggressively cleaned header: {os.path.relpath(path, BASE_DIR)}")
        with open(path, 'w', encoding='utf-8-sig') as f:
            f.write(new_content)

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
