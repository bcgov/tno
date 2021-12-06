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
	@./tools/scripts/variables.sh
	@./tools/scripts/gen-env-files.sh
	@./tools/scripts/gen-maven-files.sh
	@./tools/scripts/gen-keys.sh
	@mkdir -p ./app/editor/node_modules
	@mkdir -p ./app/subscriber/node_modules

init: ## Initialize your local environment and start the core solution.
	$(info Initialize your local environment and start the core solution)
	@make setup
	@make up
	@make db-update
	@make elastic-update

##############################################################################
# Docker Management
##############################################################################

build: ## Builds all containers or the one specified (n=service name)
	$(info Builds all containers or the one specified (n=$(n)))
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile core --profile kafka build --no-cache $(n)

up: ## Starts all containers or the one specified (n=service name)
	$(info Starts all containers or the one specified (n=$(n)))
	@docker-compose --env-file .env -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile core --profile kafka up -d $(n)

stop: ## Stops all containers or the one specified (n=service name)
	$(info Stops all containers or the one specified (n=$(n)))
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile core --profile kafka stop $(n)

down: ## Stops all containers and removes them
	$(info Stops all containers and removes them)
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile core --profile kafka down -v

restart: ## Restart all containers or the one specified (n=servic ename)
	$(info Restart all containers or the one specified (n=$(n)))
	@make stop n=$(n)
	@make up n=$(n)

refresh: ## Stop, build, and start all containers or the one specified (n=service name)
	$(info Stop, build, and start all containers or the one specified (n=$(n)))
	@make stop n=$(n)
	@make build n=$(n)
	@make up n=$(n)

remove: ## Remove all containers
	$(info Remove all containers)
	@docker-compose rm -sv

nuke: ## Stop all containers, delete all containers, volumes, and configuration
	$(info Stop all containers, delete all containers, volumes, and configuration)
	@./tools/scripts/delete-conf.sh
	@make down

##############################################################################
# Core Management
##############################################################################

core-up: ## Runs the core containers, or the one specified (n=service name)
	$(info Runs the core containers, or the one specified (n=$(n)))
	@docker-compose --env-file .env -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile core up -d $(n)

core-stop: ## Stops the core containers, or the one specified (n=service name)
	$(info Stops the core containers, or the one specified (n=$(n)))
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile core stop $(n)

##############################################################################
# Kafka Management
##############################################################################

kafka-build: ## Builds the kafka containers or the one specified (n=service name)
	$(info Builds the kafka containers or the one specified (n=$(n)))
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml  -f ./db/kafka/docker-compose.yml --profile kafka build --no-cache $(n)

kafka-up: ## Runs the kafka containers or the one specified (n=service name)
	$(info Runs the kafka containers or the one specified (n=$(n)))
	@docker-compose --env-file .env -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile kafka up -d $(n)

kafka-stop: ## Stops the kafka containers or the one specified (n=service name)
	$(info Stops the kafka containers or the one specified (n=$(n)))
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile kafka stop $(n)

kafka-down: ## Stops the kafka containers and removes them
	$(info Stops the kafka containers and removes them)
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile kafka down

kafka-rebuild: ## Build the kafka contains or the one specified (n=service name) and then start them after building
	$(info Build the kafka contains or the one specified (n=service name) and then start them after building)
	@make kafka-build n=$(n)
	@make kafka-up n=$(n)

kafka-remove: ## Remove the kafka containers
	$(info Remove the kafka containers)
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
	$(info Builds the service containers, or the one specified (n=$(n)))
	@docker-compose --env-file .env -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile service --profile utility  build $(n)

service-up: ## Runs the service containers, or the one specified (n=service name)
	$(info Runs the service containers, or the one specified (n=$(n)))
	@docker-compose --env-file .env -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile service --profile utility up -d $(n)

service-stop: ## Stops the service containers, or the one specified (n=service name)
	$(info Stops the service containers, or the one specified (n=$(n)))
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile service --profile utility stop $(n)

service-down: ## Stops the service containers and removes them
	$(info Stops the service containers and removes them)
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml --profile service --profile utility down

##############################################################################
# Node Container Management
##############################################################################

npm-down: ## Removes node containers, images, volumes, for specified application (n=service name).
	$(info Removes node containers, images, volumes, for specified application (n=$(n)))
	@make stop $(n)
	@docker-compose rm -f -v -s $(n)
	@docker volume rm -f tno-$(n)-node-cache

npm-refresh: ## Removes and rebuilds node app containers, images, volumes.
	$(info Removes and rebuilds node app containers, images, volumes)
	@make npm-down n=app-editor; make build n=app-editor; make up n=app-editor;

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
#  $(if $(r),-r,"")
# ifdef ERROR1
# 	$(error failed to run migration)
# endif

##############################################################################
# Kafka Utility Commands
##############################################################################

reset-consumer-offset: ## Reset the consumer group topic offset.
	$(info Reset the consumer group topic offset)
	@cd ./db/kafka/broker/scripts; ./reset-consumer-offset.sh

.PHONY: local
