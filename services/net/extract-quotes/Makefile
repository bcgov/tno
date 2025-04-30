# Makefile for extract-quotes service utilities

# Relative path to the prompt template file
PROMPT_TEMPLATE_FILE = LLM/Prompts/PromptTempalte.txt

# Relative path to the python script (adjust if needed)
TEXT_TO_JSON_SCRIPT = ../../../tools/scripts/text_to_json_string.py

# Ensure python3 is used, or adjust to 'python' if that's your command
PYTHON_CMD = python

# Default target: Display help
.PHONY: help
help:
	@echo "Available commands:"
	@echo "  make prompt  - Reads the prompt template, converts it to a JSON string literal, and prints it."

# Target to generate the JSON string for the prompt
.PHONY: prompt
prompt: $(PROMPT_TEMPLATE_FILE) $(TEXT_TO_JSON_SCRIPT)
	@echo "--- Generating JSON string literal for prompt template ($(PROMPT_TEMPLATE_FILE)) ---"
	@echo ""
	@echo ""
	@cat $(PROMPT_TEMPLATE_FILE) | $(PYTHON_CMD) $(TEXT_TO_JSON_SCRIPT)
	@echo ""
	@echo ""
	@echo "--- End of JSON string literal ---"
	@echo "(Copy the output above, including quotes, into your environment variable EXTRACT_QUOTES_PROMPT)"

# Basic check for file existence (optional but helpful)
$(TEXT_TO_JSON_SCRIPT):
	@echo "Error: Python script not found at $(TEXT_TO_JSON_SCRIPT)" && exit 1

$(PROMPT_TEMPLATE_FILE):
	@echo "Error: Prompt template file not found at $(PROMPT_TEMPLATE_FILE)" && exit 1 