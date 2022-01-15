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
# Solution Configuration
##############################################################################


setup: ## Setup local environment for development, generate configuration files.
	$(info Setup local environment for development, generate configuration files.)
	@./tools/scripts/gen-env-files.sh
	@./tools/scripts/gen-maven-files.sh
	@./tools/scripts/gen-keys.sh
	@mkdir -p ./app/editor/node_modules
	@mkdir -p ./app/subscriber/node_modules

init: ## Initialize your local environment and start the core solution.
	$(info Initialize your local environment and start the core solution)
	@make setup
	@make up p=init
	@make db-update
	@make elastic-update
	@make kafka-update

nuke: ## Stop all containers, delete all containers, volumes, and configuration
	$(info Stop all containers, delete all containers, volumes, and configuration)
	@make down
	@./tools/scripts/nuke.sh

##############################################################################
# Docker Management
##############################################################################

build: ## Builds all containers or the one specified (args: n={service name}, p={profile name, [all,app,kafka,service,utility,ingest]})
	$(info Builds all containers or the one specified (n=$(n), p=$(if $(p),$(p),all)))
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile $(if $(p),$(p),all) build --no-cache $(n)

up: ## Starts all containers or the one specified (args: n={service name}, p={profile name, [all,app,kafka,service,utility,ingest]}))
	$(info Starts all containers or the one specified (n=$(n), p=$(if $(p),$(p),all)))
	@docker-compose --env-file .env -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile $(if $(p),$(p),all) up -d $(n)

stop: ## Stops all containers or the one specified (args: n={service name}, p={profile name, [all,app,kafka,service,utility,ingest]}))
	$(info Stops all containers or the one specified (n=$(n), p=$(if $(p),$(p),all)))
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile $(if $(p),$(p),all) stop $(n)

down: ## Stops all containers and removes them (p={profile name, [all,app,kafka,service,utility,ingest]})))
	$(info Stops all containers and removes them (p=$(if $(p),$(p),all)))
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile $(if $(p),$(p),all) down -v

restart: ## Restart all containers or the one specified (n={service name}, p={profile name, [all,app,kafka,service,utility,ingest]}))
	$(info Restart all containers or the one specified (n=$(n), p=$(if $(p),$(p),all)))
	@make stop n=$(n) p=$(p)
	@make up n=$(n) p=$(p)

refresh: ## Stop, build, and start all containers or the one specified (args: n={service name}, p={profile name, [all,app,kafka,service,utility,ingest]}))
	$(info Stop, build, and start all containers or the one specified (n=$(n), p=$(if $(p),$(p),all)))
	@make stop n=$(n) p=$(p)
	@make build n=$(n) p=$(p)
	@make up n=$(n) p=$(p)

remove: ## Remove all containers
	$(info Remove all containers)
	@docker-compose rm -sv

##############################################################################
# Node Container Management
##############################################################################

npm-down: ## Removes node containers, images, volumes, for specified application (args: n=[editor,subscriber]).
	$(info Removes node containers, images, volumes, for specified application (n=$(if $(n),$(n),app-editor)))
	@make stop n=app-$(if $(n),$(n),editor)
	@docker-compose rm -f -v -s app-$(if $(n),$(n),editor)
	@docker volume rm -f tno-$(if $(n),$(n),editor)-node-cache

npm-refresh: ## Run yarn install within the container (args: n=[editor,subscriber]).
	$(info Run yarn install within the container (args: n=$(if $(n),$(n),app-editor)))
	@make npm-down n=$(if $(n),$(n),editor); make build n=app-$(if $(n),$(n),editor); make up n=app-$(if $(n),$(n),editor)

##############################################################################
# Database Commands
##############################################################################

db-update: ## Run the flyway database migration update (requires maven to be installed).
	$(info Run the flyway database migration update (requires maven to be installed))
	@./tools/scripts/db-update.sh

db-refresh: ## Stop and delete the database container and volume, then rebuilds and starts.
	$(info Stop and delete the database container and volume, then rebuilds and starts)
	@make stop n=keycloak
	@make stop n=database
	@docker-compose rm -f -v -s database
	@docker volume rm -f tno-database-data
	@make build n=database
	@make up n=keycloak

##############################################################################
# Elasticsearch Commands
##############################################################################

elastic-update: ## Run the elasticsearch migration (n=migration name, r=rollback, i=ignore errors).
	$(info Run the elasticsearch migration (n=$(n)))
	@./db/elasticsearch/scripts/migration.sh $(if $(n),-n $(n),"") $(if $(r),-r,"") $(if $(i),-i,"")

##############################################################################
# Kafka Commands
##############################################################################

reset-consumer-offset: ## Reset the consumer group topic offset.
	$(info Reset the consumer group topic offset)
	@cd ./db/kafka/broker/scripts; ./reset-consumer-offset.sh

kafka-update: ## Run the kafka migration (n=migration name, r=rollback, z=zookeeper)
	$(info Run the kafka migration (n=$(n)))
	@./db/kafka/scripts/migration.sh $(if $(n),-n $(n),"") $(if $(r),-r,"") $(if $(z),-z $(z),"")

.PHONY: local
