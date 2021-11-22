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
	@mvn clean

package: ## Clean and build a package.
	@echo "$(P) Clean and build a package"
	@mvn clean package

purge: ## Purge local repository.
	@echo "$(P) Purge local repository"
	@mvn dependency:purge-local-repository

install: ## Force install to fetch dependencies from remote repository.
	@echo "$(P) Force install to fetch dependencies from remote repository."
	@mvn clean install -U -X

run: ## Manually run the test project.
	@echo "$(P) Manually run the test project."
	@cd dal-db-test; mvn spring-boot:run

##############################################################################
# Maven Central Repository Commands
##############################################################################

setup: ## Import private key
	@echo "$(P) Import private key"
	@./scripts/setup.sh

deploy-staging: ## Build and deploy the package to Maven Central
	@echo "$(P) Build and deploy package to Maven Central"
	@./scripts/deploy-staging.sh

close-staging: ## Close the staging repository
	@echo "$(P) Close the staging repository"
	@./scripts/close-staging-repo.sh

release-staging: ## Release the staging repository
	@echo "$(P) Release the staging repository"
	@./scripts/release-staging-repo.sh

drop-staging: ## Drop the staging repository
	@echo "$(P) Drop the staging repository"
	@./scripts/drop-staging-repo.sh

deploy-release: ## Build and deploy the package to Maven Central and release it
	@echo "$(P) Build and deploy package to Maven Central"
	@./scripts/deploy-release.sh

##############################################################################
# Flyway Database Migration Commands
##############################################################################

db-update: ## Run the flyway database migration update
	@echo "$(P) Run the flyway database migration update"
	@./scripts/db-update.sh

.PHONY: local
