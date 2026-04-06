import requests
import sys
import io

# Ensure UTF-8 output for Vietnamese characters in Windows terminal
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

def translate_text(text, target_lang="vi", source_lang="en"):
    """
    Translates text using the MyMemory API (Free tier).
    :param text: Text to translate
    :param target_lang: Target language code (e.g., 'vi', 'en')
    :param source_lang: Source language code (e.g., 'en', 'vi')
    :return: Translated text
    """
    if not text:
        return ""
    
    url = f"https://api.mymemory.translated.net/get?q={text}&langpair={source_lang}|{target_lang}"
    
    try:
        response = requests.get(url)
        response.raise_for_status()
        data = response.json()
        return data["responseData"]["translatedText"]
    except Exception as e:
        return f"[Translation Error] {e}"

if __name__ == "__main__":
    if len(sys.argv) > 1:
        # Get text from command line arguments
        input_text = " ".join(sys.argv[1:])
        # Default translation to Vietnamese
        result = translate_text(input_text, "vi", "en")
        print(f"Original: {input_text}")
        print(f"Translated: {result}")
    else:
        # Example usage
        test_text = "Hello, how can I help you today?"
        print(f"Example: '{test_text}' -> '{translate_text(test_text, 'vi', 'en')}'")
