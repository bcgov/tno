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

setup: ## Setup local environment for development, generate .env files.
	@echo "$(P) Setup local development environment"
	@./tools/scripts/gen-env-files.sh

##############################################################################
# Docker Management
##############################################################################

up: ## Runs the local containers or the one specified (n=service name)
	@echo "$(P) Running containers..."
	@docker-compose --env-file .env -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml up -d $(n)

down: ## Stops the local containers and removes them
	@echo "$(P) Stopping containers..."
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml down

stop: ## Stops the local containers or the one specified (n=service name)
	@echo "$(P) Stopping containers..."
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml stop $(n)

build: ## Builds the local containers or the one specified (n=service name)
	@echo "$(P) Building images..."
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml build --no-cache $(n)

rebuild: ## Build the local contains or the one specified (n=service name) and then start them after building
	@make build n=$(n)
	@make up n=$(n)

remove: ## Remove all the local containers
	@echo "$(P) Removing images..."
	@docker-compose rm -sv

##############################################################################
# Non-Kafka Management
##############################################################################

base-up: ## Runs the local containers without Kafka or the one specified (n=service name)
	@echo "$(P) Running containers..."
	@docker-compose --env-file .env -f docker-compose.yml -f docker-compose.override.yml up -d $(n)

base-stop: ## Stops the local containers without stopping Kafka or the one specified (n=service name)
	@echo "$(P) Stopping containers..."
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml stop $(n)

##############################################################################
# Kafka Management
##############################################################################

kafka-up: ## Runs the local kafka containers or the one specified (n=service name)
	@echo "$(P) Running kafka containers..."
	@docker-compose --env-file .env -f docker-compose.override.yml -f ./db/kafka/docker-compose.yml up -d $(n)

kafka-down: ## Stops the local kafka containers and removes them
	@echo "$(P) Stopping kafka containers..."
	@docker-compose -f docker-compose.override.yml  -f ./db/kafka/docker-compose.yml down

kafka-stop: ## Stops the local kafka containers or the one specified (n=service name)
	@echo "$(P) Stopping kafka containers..."
	@docker-compose -f docker-compose.override.yml  -f ./db/kafka/docker-compose.yml stop $(n)

kafka-build: ## Builds the local kafka containers or the one specified (n=service name)
	@echo "$(P) Building kafka images..."
	@docker-compose -f docker-compose.override.yml  -f ./db/kafka/docker-compose.yml build --no-cache $(n)

kafka-rebuild: ## Build the local kafka contains or the one specified (n=service name) and then start them after building
	@make kafka-build n=$(n)
	@make kafka-up n=$(n)

kafka-remove: ## Remove the local kafka containers
	@echo "$(P) Removing kafka images..."
	@docker rm -fv tno-rest-proxy
	@docker rm -fv tno-ksql-datagen
	@docker rm -fv tno-ksqldb-cli
	@docker rm -fv tno-schema-registry
	@docker rm -fv tno-control-center
	@docker rm -fv tno-ksqldb-server
	@docker rm -fv tno-connect
	@docker rm -fv tno-zookeeper
	@docker rm -fv tno-broker

##############################################################################
# Node Container Management
##############################################################################

npm-down: ## Removes containers, images, volumes, for specified application (n=service name).
	@echo "$(P) Removing node containers and volumes."
	@docker-compose stop $(n)
	@docker-compose rm -f -v -s $(n)
	@docker volume rm -f tno-$(n)-node-cache

npm-refresh: ## Removes and rebuilds containers, images, volumes, for specified application (n=service name).
	@make npm-down; make build n=$(n); make up $(n);

.PHONY: local
