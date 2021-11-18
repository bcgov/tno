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
	@echo "$(P) Setup local development environment"
	@./tools/scripts/gen-env-files.sh
	@./tools/scripts/gen-dal-db-files.sh
	@./tools/scripts/gen-keys.sh
	@mkdir -p ./app/editor/node_modules
	@mkdir -p ./app/subscriber/node_modules

##############################################################################
# Docker Management
##############################################################################

build: ## Builds all containers or the one specified (n=service name)
	@echo "$(P) Building images..."
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml build --no-cache $(n)

up: ## Starts all containers or the one specified (n=service name)
	@echo "$(P) Starting all containers..."
	@docker-compose --env-file .env -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile core --profile kafka up -d $(n)

stop: ## Stops all containers or the one specified (n=service name)
	@echo "$(P) Stopping all containers..."
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml stop $(n)

down: ## Stops all containers and removes them
	@echo "$(P) Stopping containers..."
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml down

restart: ## Stop and start all containers or the one specified (n=servic ename)
	@echo "$(P) Restarting containers..."
	@make stop n=$(n)
	@make up n=$(n)

rebuild: ## Rebuild all containers or the one specified (n=service name) and then start them after building
	@make build n=$(n)
	@make up n=$(n)

remove: ## Remove all containers
	@echo "$(P) Removing images..."
	@docker-compose rm -sv

##############################################################################
# Core Management
##############################################################################

core-up: ## Runs the core containers, or the one specified (n=service name)
	@echo "$(P) Running containers..."
	@docker-compose --env-file .env -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile core up -d $(n)

core-stop: ## Stops the core containers, or the one specified (n=service name)
	@echo "$(P) Stopping containers..."
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile core stop $(n)

##############################################################################
# Kafka Management
##############################################################################

kafka-build: ## Builds the kafka containers or the one specified (n=service name)
	@echo "$(P) Building kafka images..."
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml  -f ./db/kafka/docker-compose.yml --profile kafka build --no-cache $(n)

kafka-up: ## Runs the kafka containers or the one specified (n=service name)
	@echo "$(P) Running kafka containers..."
	@docker-compose --env-file .env -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile kafka up -d $(n)

kafka-stop: ## Stops the kafka containers or the one specified (n=service name)
	@echo "$(P) Stopping kafka containers..."
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile kafka stop $(n)

kafka-down: ## Stops the kafka containers and removes them
	@echo "$(P) Stopping kafka containers..."
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile kafka down

kafka-rebuild: ## Build the kafka contains or the one specified (n=service name) and then start them after building
	@make kafka-build n=$(n)
	@make kafka-up n=$(n)

kafka-remove: ## Remove the kafka containers
	@echo "$(P) Removing kafka images..."
	@docker rm -fv tno-kafkacat
	@docker rm -fv tno-ksqldb
	@docker rm -fv tno-cconnect
	@docker rm -fv tno-rest-proxy
	@docker rm -fv tno-schema-registry
	@docker rm -fv tno-broker
	@docker rm -fv tno-zookeeper

##############################################################################
# Service Management
##############################################################################

service-build: ## Builds the service containers, or the one specified (n=service name)
	@echo "$(P) Building service containers..."
	@docker-compose --env-file .env -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile service --profile utility  build $(n)

service-up: ## Runs the service containers, or the one specified (n=service name)
	@echo "$(P) Running service containers..."
	@docker-compose --env-file .env -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile service --profile utility up -d $(n)

service-stop: ## Stops the service containers, or the one specified (n=service name)
	@echo "$(P) Stopping service containers..."
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile service --profile utility stop $(n)

service-down: ## Stops the service containers and removes them
	@echo "$(P) Stopping service containers..."
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile service --profile utility down

##############################################################################
# Node Container Management
##############################################################################

npm-down: ## Removes node containers, images, volumes, for specified application (n=service name).
	@echo "$(P) Removing node containers and volumes."
	@make stop $(n)
	@docker-compose rm -f -v -s $(n)
	@docker volume rm -f tno-$(n)-node-cache

npm-refresh: ## Removes and rebuilds node app containers, images, volumes.
	@make npm-down n=app-editor; make build n=app-editor; make up n=app-editor;

##############################################################################
# Flyway Database Migration Commands
##############################################################################

db-update: ## Run the flyway database migration update (requires maven to be installed).
	@echo "$(P) Run the flyway database migration update"
	@cd libs/java/dal/db; make db-update;

##############################################################################
# Kafka Utility Commands
##############################################################################

reset-consumer-offset: ## Reset the consumer group topic offset.
	@echo "$(P) Reset the consumer group topic offset"
	@cd ./db/kafka/broker/scripts; ./reset-consumer-offset.sh

.PHONY: local
