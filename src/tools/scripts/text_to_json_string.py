import json
import sys

"""
Converts text read from standard input into a valid JSON string literal.
Handles escaping of necessary characters (double quotes, backslashes, control chars).

Usage:
    Pass the text you want to convert via standard input.

    Example (Linux/macOS/WSL):
        cat your_prompt_file.txt | python text_to_json_string.py
        echo "Line 1\nLine 2 with \"quotes\" and a backslash \\" | python text_to_json_string.py

Output:
    The script prints a single line containing the JSON-escaped string literal,
    including the surrounding double quotes. You can copy this output and paste
    it directly as the value for a field in your JSON file.
"""

def escape_text_for_json(input_text):
  """Converts a Python string to a JSON-escaped string literal."""
  # json.dumps handles all necessary escaping according to JSON standard.
  # ensure_ascii=False prevents escaping of non-ASCII characters (e.g., Chinese),
  # keeping them readable in the output string.
  return json.dumps(input_text, ensure_ascii=False)

if __name__ == "__main__":
    try:
        # Read all text from standard input
        original_text = sys.stdin.read()

        # Convert to JSON string literal
        json_string = escape_text_for_json(original_text)

        # Print the resulting JSON string literal to standard output
        print(json_string)

    except Exception as e:
        # Print errors to stderr
        print(f"An error occurred: {e}", file=sys.stderr)
        sys.exit(1) 