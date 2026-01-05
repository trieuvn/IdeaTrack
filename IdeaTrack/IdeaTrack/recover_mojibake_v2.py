# -*- coding: utf-8 -*-
"""recover_mojibake_v2.py
Attempts to recover files corrupted by recursive UTF-8 -> CP1252 -> UTF-8 encoding.
"""
import os, codecs

BASE_DIR = r'd:\aDesktop\Dotnet\Repo\IdeaTrack\IdeaTrack'

def unwrap(text):
    try:
        # Encode as cp1252 to get the raw bytes, then decode as utf-8 to get the original chars
        return text.encode('cp1252').decode('utf-8')
    except (UnicodeEncodeError, UnicodeDecodeError):
        return None

def heal_file(path):
    try:
        with open(path, 'r', encoding='utf-8') as f:
            content = f.read()
    except UnicodeDecodeError:
        try:
            with open(path, 'r', encoding='utf-8-sig') as f:
                content = f.read()
        except:
            return

    original = content
    current = content
    
    # Try up to 5 levels
    for i in range(5):
        fixed = unwrap(current)
        if fixed and len(fixed) < len(current):
            current = fixed
        else:
            break
            
    if current != original:
        print(f"Recovered: {os.path.relpath(path, BASE_DIR)}")
        with open(path, 'w', encoding='utf-8-sig') as f:
            f.write(current)

def main():
    target_dirs = [
        os.path.join(BASE_DIR, 'Areas', 'SciTech'),
        # Add others if needed
    ]
    
    for d in target_dirs:
        for root, _, files in os.walk(d):
            for f in files:
                if f.endswith('.cshtml') or f.endswith('.cs'):
                    heal_file(os.path.join(root, f))

if __name__ == '__main__':
    main()
