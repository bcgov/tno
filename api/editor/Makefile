#!/usr/bin/make

SHELL := /usr/bin/env bash
.DEFAULT_GOAL := help

ifneq ($(OS),Windows_NT)
POSIXSHELL := 1
else
POSIXSHELL :=
endif

# to see all colors, run
# bash -c 'for c in {0..255}; do tput setaf $c; tput setaf $c | cat -v; echo =$c; done'
# the first 15 entries are the 8-bit colors

# define standard colors
BLACK        := $(shell tput -Txterm setaf 0)
RED          := $(shell tput -Txterm setaf 1)
GREEN        := $(shell tput -Txterm setaf 2)
YELLOW       := $(shell tput -Txterm setaf 3)
LIGHTPURPLE  := $(shell tput -Txterm setaf 4)
PURPLE       := $(shell tput -Txterm setaf 5)
BLUE         := $(shell tput -Txterm setaf 6)
WHITE        := $(shell tput -Txterm setaf 7)

RESET := $(shell tput -Txterm sgr0)

# default "prompt"
P = ${GREEN}[+]${RESET}

help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' Makefile | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-30s\033[0m %s\n", $$1, $$2}'

.PHONY: help

##############################################################################
# Maven Commands
##############################################################################

clean: ## Clean package.
	@echo "$(P) Clean package"
	@cd api-editor; mvn clean

package: ## Clean and build a package.
	@echo "$(P) Clean and build a package"
	@cd api-editor; mvn clean package

purge: ## Purge local repository.
	@echo "$(P) Purge local repository"
	@cd api-editor; mvn dependency:purge-local-repository
	
install: ## Force install to fetch dependencies from remote repository.
	@echo "$(P) Force install to fetch dependencies from remote repository."
	@cd api-editor; mvn clean install -U -X
	
run: ## Manually run the project.
	@echo "$(P) Manually run the project."
	@cd api-editor; mvn spring-boot:run

.PHONY: local
