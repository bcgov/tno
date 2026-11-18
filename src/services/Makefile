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
# Dotnet
##############################################################################

clean: ## Clean compiled artifacts from solution.
	$(info Clean compiled artifacts from solution)
	@dotnet clean

restore: ## Restore dependendencies.
	$(info Restore dependendencies)
	@dotnet restore

build: ## Build solution.
	$(info Build solution)
	@dotnet build

##############################################################################
# Dotnet Tools
##############################################################################

tool-update: ## Update tool cli (n=name of package)
	$(info Update tool cli (n=$(if $(n),$(n),dotnet-ef)))
	@dotnet tool update --global $(if $(n),$(n),dotnet-ef)

##############################################################################
# Entity Framework
##############################################################################

db-migrations: ## Display a list of migrations.
	$(info Display a list of migrations.)
	@cd dal; dotnet ef migrations list

db-add: ## Add a new database migration for the specified name (n=name of migration).
	$(info Create a new database migration for the specified name.)
	@cd dal; dotnet ef migrations add $(n); code -r ./Migrations/*_$(n).cs
	@./scripts/db-migration.sh $(n);

db-update: ## Update the database with the latest migration (n=name of migration).
	$(info Updating database with latest migration...)
	@cd dal; dotnet ef database update ${n};

# db-update: ## Update the database with the latest migration.
# 	$(info Updating database with latest migration...)
# 	@docker-compose --env-file .env up -d database; cd backend/dal; dotnet ef database update

db-rollback: ## Rollback to the specified database migration (n=name of migration).
	$(info Rollback to the specified database migration.)
	@cd dal; dotnet ef database update $(n);

db-remove: ## Remove the last database migration.
	$(info Remove the last migration.)
	@cd dal; dotnet ef migrations remove --force;

db-clean: ## Drop the database and refresh.
	$(info Drop the database and refresh.)
	@cd dal; dotnet ef database drop --force; dotnet ef database update

db-refresh: ## Rollback to a blank database and rerun the database migrations.
	$(info Rollback to a blank database and rerun the database migrations.)
	@make db-rollback n=0
	@make db-update

db-drop: ## Drop the database.
	$(info Drop the database.)
	@cd dal; dotnet ef database drop;

db-script: ## Export an SQL script from the migration (from=0 to=Initial).
	$(info Exporting script to 'db-migration.sql')
	@cd dal; dotnet ef migrations script ${from} ${to} --output ../../db-migration.sql

db-recreate: ## Rollback to the specified migration, remove the last migration, create a new migration, update the database (r=name of rollback migration, n=name of migration).
	$(info Rollback to the specified migration, remove the last migration, create a new migration, update the database (r=$(if $(r),$(r),0), n=$(if $(n),$(n),Initial)))
	@make db-rollback n=$(if $(r),$(r),0)
	@make db-remove
	@make db-add n=$(if $(n),$(n),Initial)
	@make db-update

.PHONY: local
