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

NAMESPACE=9b301c

help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' Makefile | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-30s\033[0m %s\n", $$1, $$2}'

.PHONY: help

##############################################################################
# Openshift Helper Methods
##############################################################################

switch: ## Switch active project (n=[dev,test,prod,tools]).
	$(info Switch active project n=$(NAMESPACE)-$(if $(n),$(n),tools))
	@oc project $(NAMESPACE)-$(if $(n),$(n),tools)

clean: ## Delete old pods that are no longer running (n=[dev,test,prod,tools]).
	$(info Delete old pods that are no longer running n=$(NAMESPACE)-$(if $(n),$(n),dev))
	@oc delete pod --field-selector=status.phase==Succeeded -n $(NAMESPACE)-$(if $(n),$(n),dev)

pod: ## Get the full pod names for the specified Deployment (n=label name).
	$(info Get the pod names for the label [name=$(n)])
	@cd ./scripts; ./get-pod-name.sh $(n)

logs: ## Show logs for all containers within a pod the label (n=label name, i=pod index).
	$(info Show logs for all containers within a pod with the label [name=$(n)])
	@cd ./scripts; ./get-logs.sh $(n) $(i)

rsh: ## Remote shell into pod with the label (l=label, i=pod index).
	$(info Remote shell into pod with the label [l=$(l), i=$(i)])
	@cd ./scripts; ./rsh.sh $(l) $(i)

port-forward: ## Port forward to a pod (n=label name, i=pod index, l=local port, r=remote port).
	$(info Port forward to a port [name=$(n)] $(l):$(r))
	@cd ./scripts; ./port-forward.sh $(n) $(if $(i),$(i),0) $(l) $(r)

db-connect: ## Port forward to the database (e=environment, l=local port, r=remote port).
	$(info Port forward to the database [e=$(if $(e),$(e),dev)] $(l):$(r))
	@cd ./scripts; ./database-connect.sh $(if $(e),$(e),dev) $(l) $(r)

deploy: ## Deploy to environment (e=environment).
	$(info Deploy to environment (e=environment) [e=$(if $(e),$(e),dev), t=$(if $(t),$(t),latest)])
	@cd ./scripts; ./deploy.sh $(if $(e),$(e),dev) $(if $(t),$(t),latest)

stop: ## Stop environment (e=environment).
	$(info Stop environment (e=environment) [e=$(if $(e),$(e),dev)])
	@cd ./scripts; ./stop.sh $(if $(e),$(e),dev)

up: ## Start up environment (e=environment).
	$(info Start up environment (e=environment) [e=$(if $(e),$(e),dev)])
	@cd ./scripts; ./up.sh $(if $(e),$(e),dev)

annotate: ## Annotate an object.
	$(info Annotate an object (n=name) [e=environment])
	@cd ./scripts; ./annotate.sh $(if $(e),$(e),dev) $(n)

.PHONY: local
