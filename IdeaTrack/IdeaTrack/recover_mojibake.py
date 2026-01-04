# -*- coding: utf-8 -*-
"""recover_mojibake.py
Attempts to recover files corrupted by recursive UTF-8 -> Latin-1 -> UTF-8 encoding.
It recursively specific directories (Area/SciTech) and tries to unwrap the encoding layers.
"""
import os, codecs

BASE_DIR = r'd:\aDesktop\Dotnet\Repo\IdeaTrack\IdeaTrack'

# Function to attempt one layer of unwrapping
def unwrap(text):
    try:
        # Encode as latin-1 to get back the bytes that were wrongly interpreted as characters
        # But since it was saved as UTF-8, the characters we see (like 'Ã') are the UTF-8 representations.
        # We need to take the string, encode as 'latin-1' (which maps e.g. Ã back to 0xC3),
        # providing we are reversing the Read-As-Latin1-Save-As-UTF8 process?
        # Actually, if we read as Latin-1 and saved as UTF-8:
        # Original: "Tiếng" (UTF-8 bytes: 54 69 C3 AA 6E 67)
        # Read as Latin-1: "TiÃªng" (chars: T, i, Ã, ª, n, g)
        # Save as UTF-8: bytes for T,i,Ã,ª,n,g -> 54 69 C3 83 C2 AA 6E 67
        # So to reverse: Read as UTF-8 (gives "TiÃªng"), encode as Latin-1 (gives bytes 54...67), decode as UTF-8 (gives "Tiếng").
        
        # However, the corruption looks deeper: "AƒAƒA‚..."
        # This implies multiple layers.
        
        return text.encode('latin-1').decode('utf-8')
    except (UnicodeEncodeError, UnicodeDecodeError):
        return None

def heal_file(path):
    try:
        with open(path, 'r', encoding='utf-8') as f:
            content = f.read()
    except UnicodeDecodeError:
        print(f"Cannot read {path} as UTF-8")
        return

    original = content
    current = content
    
    # Try up to 5 levels of un-wrapping
    for i in range(5):
        fixed = unwrap(current)
        if fixed and len(fixed) < len(current): # Heuristic: repairing usually shortens the string (2 bytes -> 1 char)
            current = fixed
        else:
            break
            
    if current != original:
        print(f"Recovered: {os.path.relpath(path, BASE_DIR)}")
        with open(path, 'w', encoding='utf-8-sig') as f:
            f.write(current)

def main():
    # Only target the areas we likely messed up
    target_dirs = [
        os.path.join(BASE_DIR, 'Areas', 'SciTech'),
        os.path.join(BASE_DIR, 'Areas', 'Author'), # Just in case
        os.path.join(BASE_DIR, 'Areas', 'Admin'),  # Just in case
    ]
    
    for d in target_dirs:
        for root, _, files in os.walk(d):
            for f in files:
                if f.endswith('.cshtml') or f.endswith('.cs'):
                    heal_file(os.path.join(root, f))

if __name__ == '__main__':
    main()
