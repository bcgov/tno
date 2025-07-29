#!/usr/bin/make

SHELL := /usr/bin/env bash
.DEFAULT_GOAL := help

# Color definitions
GREEN  := \033[0;32m
YELLOW := \033[0;33m
RED    := \033[0;31m
RESET  := \033[0m

# Default prompt
P = ${GREEN}[+]${RESET}

# Python environment directory
VENV_DIR := .venv
VENV_BIN := $(VENV_DIR)/Scripts
PYTHON := $(VENV_BIN)/python.exe
UV := uv
ACTIVATE_SCRIPT := $(VENV_BIN)/activate

# Check if we're in an active virtual environment
ifneq (,$(VIRTUAL_ENV))
    IN_VENV = yes
else
    IN_VENV = no
endif

# Windows-specific path handling
ifdef OS
    ifeq ($(OS),Windows_NT)
        # For Windows, just use the relative path
        VENV_ACTIVATE := $(subst \,/,./$(VENV_BIN)/activate)
    endif
else
    VENV_ACTIVATE := $(ACTIVATE_SCRIPT)
endif

.PHONY: help activate deactivate install clean check-uv build

help: ## Show this help message
	@echo -e "${GREEN}Available targets:${RESET}"
	@echo -e "  make ${YELLOW}activate${RESET}    Create and activate Python virtual environment using uv"
	@echo -e "  make ${YELLOW}deactivate${RESET}  Deactivate the current virtual environment"
	@echo -e "  make ${YELLOW}install${RESET}     Install dependencies using uv sync"
	@echo -e "  make ${YELLOW}build${RESET}       Build the project using build.py"
	@echo -e "  make ${YELLOW}clean${RESET}       Clean up temporary files and virtual environment"

check-uv:
	@if ! command -v $(UV) &> /dev/null; then \
		echo -e "${YELLOW}[!] uv is not installed. Installing uv...${RESET}"; \
		curl -sSfL https://astral.sh/uv/install.sh | sh; \
		echo -e "${GREEN}[+] Please restart your shell and run 'make activate' again.${RESET}"; \
		exit 1; \
	fi

$(VENV_DIR)/pyvenv.cfg: pyproject.toml
	@echo -e "${P} Creating Python virtual environment with uv..."
	@$(UV) venv $(VENV_DIR)
	@echo -e "${P} Syncing dependencies..."
	@source "$(ACTIVATE_SCRIPT)" && $(UV) sync

activate: check-uv $(VENV_DIR)/pyvenv.cfg  ## Create and activate Python virtual environment using uv
	@if [ "$(IN_VENV)" = "no" ]; then \
		echo -e "${GREEN}[+] Virtual environment is ready!${RESET}"; \
		echo -e "${GREEN}To activate, run:${RESET}"; \
		echo -e "  source $(ACTIVATE_SCRIPT)"; \
		echo -e "${GREEN}Or on Windows CMD:${RESET}"; \
		echo -e "  $(subst /,\\,$(ACTIVATE_SCRIPT))"; \
		echo -e "${GREEN}To deactivate, run: deactivate${RESET}"; \
	else \
		echo -e "${YELLOW}[!] Already in virtual environment.${RESET}"; \
	fi

install: check-uv  ## Install dependencies using uv sync
	@if [ -f "$(ACTIVATE_SCRIPT)" ]; then \
		echo -e "${P} Syncing dependencies..."; \
		source "$(ACTIVATE_SCRIPT)" && $(UV) sync; \
	else \
		make activate; \
	fi

deactivate:  ## Deactivate the current virtual environment
	@if [ -n "$(VIRTUAL_ENV)" ]; then \
		echo -e "${GREEN}[+] Run this command to deactivate:${RESET}"; \
		echo -e "  deactivate"; \
	else \
		echo -e "${YELLOW}[!] No active virtual environment to deactivate.${RESET}"; \
	fi

clean:  ## Clean up temporary files and virtual environment
	@echo -e "${P} Cleaning up..."
	@rm -rf "$(VENV_DIR)" __pycache__ .pytest_cache 2>/dev/null || true
	@find . -type d -name "__pycache__" -exec rm -rf {} + 2>/dev/null || true
	@find . -type f -name "*.py[co]" -delete 2>/dev/null || true

build: check-uv  ## Build the project using build.py
	@if [ ! -f "build.py" ]; then \
		echo -e "${RED}[!] build.py not found in current directory.${RESET}"; \
		exit 1; \
	fi
	@if [ -f "$(ACTIVATE_SCRIPT)" ]; then \
		echo -e "${P} Building project..."; \
		source "$(ACTIVATE_SCRIPT)" && $(PYTHON) build.py; \
	else \
		echo -e "${YELLOW}[!] Virtual environment not found. Creating it first...${RESET}"; \
		make activate; \
		echo -e "${P} Building project..."; \
		source "$(ACTIVATE_SCRIPT)" && $(PYTHON) build.py; \
	fi
