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
	@mkdir -p ./app/editor/node_modules
	@mkdir -p ./app/subscriber/node_modules

nuke: ## Stop all containers, delete all containers, volumes, and configuration
	$(info Stop all containers, delete all containers, volumes, and configuration)
	@make down
	@./tools/scripts/nuke.sh

##############################################################################
# Openshift Management
##############################################################################

project: ## Switch to TNO project namespace within Openshift: 9b301c
	$(info Switch to TNO project namespace within Openshift: 9b301c)
	@oc project 9b301c-tools

switch: ## Switch to the specified oc project (n=environment)
	$(info Switch to the specified oc project n=$(if $(n),$(n),tools))
	@./tools/scripts/switch-project.sh $(n)

##############################################################################
# Docker Management
##############################################################################

build: ## Builds all containers or the one specified (args: n={service name}, p={profile name, [all,api,editor,subscriber,kafka,service,utility,ingest]})
	$(info Builds all containers or the one specified (n=$(n), $(p)))
	@docker-compose \
		-f docker-compose.yml \
		-f docker-compose.override.yml \
		-f ./db/kafka/docker-compose.yml \
		-f ./services/docker-compose.yml \
		$(if $(p),--profile $(p),$(if $(n),--profile all,)) \
		build --no-cache --force-rm $(n)

up: ## Starts all containers or the one specified (args: n={service name}, p={profile name, [all,api,editor,subscriber,kafka,service,utility,ingest]}))
	$(info Starts all containers or the one specified (n=$(n), p=$(p)))
	@docker-compose \
		--env-file .env \
		-f docker-compose.yml \
		-f docker-compose.override.yml \
		-f ./db/kafka/docker-compose.yml \
		-f ./services/docker-compose.yml \
		$(if $(p),--profile $(p),$(if $(n),--profile all,)) \
		up -d $(n)

stop: ## Stops all containers or the one specified (args: n={service name}, p={profile name, [all,api,editor,subscriber,kafka,service,utility,ingest]}))
	$(info Stops all containers or the one specified (n=$(n), p=$(p)))
	@docker-compose \
		-f docker-compose.yml \
		-f docker-compose.override.yml \
		-f ./db/kafka/docker-compose.yml \
		-f ./services/docker-compose.yml \
		$(if $(p),--profile $(p),$(if $(n),--profile all,--profile all)) \
		stop $(n)

down: ## Stops all containers and removes them (p={profile name, [all,api,editor,subscriber,kafka,service,utility,ingest]})))
	$(info Stops all containers and removes them (p=$(p)))
	@docker-compose \
		-f docker-compose.yml \
		-f docker-compose.override.yml \
		-f ./db/kafka/docker-compose.yml \
		-f ./services/docker-compose.yml \
		$(if $(p),--profile $(p),$(if $(n),--profile all,)) \
		down -v

restart: ## Restart all containers or the one specified (n={service name}, p={profile name, [all,api,editor,subscriber,kafka,service,utility,ingest]}))
	$(info Restart all containers or the one specified (n=$(n), p=$(p)))
	@make stop $(if $(n),n=$(n),) $(if $(p),p=$(p),)
	@make up $(if $(n),n=$(n),) $(if $(p),p=$(p),)

refresh: ## Stop, build, and start all containers or the one specified (args: n={service name}, p={profile name, [all,api,editor,subscriber,kafka,service,utility,ingest]}))
	$(info Stop, build, and start all containers or the one specified (n=$(n), p=$(p)))
	@make stop $(if $(n),n=$(n),) $(if $(p),p=$(p),)
	@./tools/scripts/docker-remove.sh $(n)
	@make build $(if $(n),n=$(n),) $(if $(p),p=$(p),)
	@make up $(if $(n),n=$(n),) $(if $(p),p=$(p),)

refresh-keycloak: ## Stop, build, drop DB and recreate
	$(info Stop, build, drop DB and recreate)
	@make stop n=keycloak
	@./tools/scripts/docker-remove.sh keycloak
	@./tools/scripts/refresh-keycloak.sh
	@make build n=keycloak
	@make up n=keycloak

remove: ## Remove all containers
	$(info Remove all containers)
	@docker-compose rm -sv

renew: ## Refresh all relevant services that were impacted by prior Pull Request.
	$(info Refresh all relevant services that were impacted by prior Pull Request.)
	@make stop n=indexing
	@make refresh n=api
	@make refresh n=editor
	@make refresh n=subscriber
	@make db-update
	@make elastic-update
	@make refresh n=indexing

##############################################################################
# Database Commands
##############################################################################

db-update: ## Run the database migration update.
	$(info Run the database migration update)
	@./tools/scripts/db-update.sh

db-refresh: ## Drop database and reinitialize
	$(info Drop database and reinitialize)
	@./tools/scripts/db-drop.sh
	@make db-update

db-nuke: ## Stop and delete the database container and volume, then rebuild and start.
	$(info Stop and delete the database container and volume, then rebuild and start)
	@make stop n=keycloak
	@make stop n=database
	@docker rm -f -v tno-database
	@docker volume rm -f tno-database-data
	@make up n=database
	@make db-update
	@make up n=keycloak

##############################################################################
# Elasticsearch Commands
##############################################################################

elastic-update: ## Run the elasticsearch migration.
	$(info Run the elasticsearch migration)
	@./tools/scripts/elastic-update.sh

##############################################################################
# Kafka Commands
##############################################################################

reset-consumer-offset: ## Reset the consumer group topic offset.
	$(info Reset the consumer group topic offset)
	@cd ./db/kafka/scripts/reset-consumer-offset.sh

kafka-update: ## Run the kafka migration (n=migration name, r=rollback, b=bootstrap server, p=partitions)
	$(info Run the kafka migration (n=$(n)))
	@./db/kafka/scripts/migration.sh $(if $(n),-n $(n),) $(if $(r),-r,) $(if $(b),-b $(b),) $(if $(p),-p $(p),)

kafka-topic-add: ## Add a new kafka topic (e=environment, t=topic, b=bootstrap server, p=partitions, r=replications, i=index)
	$(info Add a new kafka topic (e=$(if $(e),$(e),local), t=$(t)))
	@./db/kafka/scripts/topic-add.sh $(if $(e),-e $(e),) $(if $(t),-t $(t),) $(if $(b),-b $(b),) $(if $(p),-p $(p),) $(if $(r),-r $(r),) $(if $(i),-i $(i),)

kafka-topic-delete: ## Delete a kafka topic (t=topic, b=bootstrap server)
	$(info Add a new kafka topic (e=$(if $(e),$(e),local), t=$(t)))
	@./db/kafka/scripts/topic-add.sh $(if $(e),-e $(e),) $(if $(t),-t $(t),) $(if $(b),-b $(b),)

.PHONY: local
