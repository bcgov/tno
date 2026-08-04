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
	@echo "Usage: make <target> [argument=value ...]   (e.g. make build n=api t=dev)"
	@echo "Argument notation: {a,b,c} allowed values, [x] default when omitted,"
	@echo "                   an argument with no default is required."
	@echo
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' Makefile | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-30s\033[0m %s\n", $$1, $$2}'

.PHONY: help

##############################################################################
# Openshift Helper Methods
##############################################################################

switch: ## Switch active project (n=environment {dev,test,prod,tools} [tools]).
	$(info Switch active project n=$(NAMESPACE)-$(if $(n),$(n),tools))
	@oc project $(NAMESPACE)-$(if $(n),$(n),tools)

clean: ## Delete old pods that are no longer running (n=environment {dev,test,prod,tools} [dev]).
	$(info Delete old pods that are no longer running n=$(NAMESPACE)-$(if $(n),$(n),dev))
	@oc delete pod --field-selector=status.phase==Succeeded -n $(NAMESPACE)-$(if $(n),$(n),dev)

pod: ## Get the full pod names matching a label (n=label selector, e.g. name=api).
	$(info Get the pod names for the label [name=$(n)])
	@cd ./scripts; ./get-pod-name.sh $(n)

logs: ## Show logs for all containers within a pod (n=label selector, i=pod index [0]).
	$(info Show logs for all containers within a pod with the label [name=$(n)])
	@cd ./scripts; ./get-logs.sh $(n) $(i)

rsh: ## Remote shell into a pod (l=label selector, i=pod index [0]).
	$(info Remote shell into pod with the label [l=$(l), i=$(i)])
	@cd ./scripts; ./rsh.sh $(l) $(i)

port-forward: ## Port forward to a pod (n=label selector, l=local port, r=remote port, i=pod index [0]).
	$(info Port forward to a port [name=$(n)] $(l):$(r))
	@cd ./scripts; ./port-forward.sh $(n) $(if $(i),$(i),0) $(l) $(r)

db-connect: ## Port forward to the database (e=environment {dev,test,prod,tools} [dev], l=local port [22222], r=remote port [5432]).
	$(info Port forward to the database [e=$(if $(e),$(e),dev)] $(l):$(r))
	@cd ./scripts; ./database-connect.sh $(if $(e),$(e),dev) $(l) $(r)

deploy: ## Deploy to environment - retags images to e and restarts (e=environment {dev,test,prod} [dev], t=source tag to promote [latest], n=single workload name, e.g. charts-api or db-migration [all], m=migration target for n=db-migration, 0 reverts all [apply all pending], s=db credential secret for n=db-migration, e.g. montford [whatever the api uses]).
	$(info Deploy to environment (e=environment) [e=$(if $(e),$(e),dev), t=$(if $(t),$(t),latest), n=$(if $(n),$(n),all)$(if $(m), m=$(m),)$(if $(s), s=$(s),)])
	@cd ./scripts; ./deploy.sh "$(if $(e),$(e),dev)" "$(if $(t),$(t),latest)" "$(n)" "$(m)" "$(s)"

stop: ## Stop environment - scales every service to 0 (e=environment {dev,test,prod} [dev]).
	$(info Stop environment (e=environment) [e=$(if $(e),$(e),dev)])
	@cd ./scripts; ./stop.sh $(if $(e),$(e),dev)

up: ## Start up environment - scales every service to its per-environment replica count (e=environment {dev,test,prod} [dev]).
	$(info Start up environment (e=environment) [e=$(if $(e),$(e),dev)])
	@cd ./scripts; ./up.sh $(if $(e),$(e),dev)

annotate: ## Annotate an object to force a rollout (n=object type/name, e.g. dc/corenlp, e=environment {dev,test,prod} [dev]).
	$(info Annotate an object (n=name) [e=environment])
	@cd ./scripts; ./annotate.sh $(if $(e),$(e),dev) $(n)

##############################################################################
# Azure Container Registry Images
##############################################################################

build: ## Build a linux/amd64 image tagged for ACR (n=component, t=tag [latest], e=Dockerfile variant {prod,local} [prod]).
	$(info Build image [n=$(n), t=$(if $(t),$(t),latest), e=$(if $(e),$(e),prod)])
	@cd ./scripts; ./build.sh "$(n)" "$(if $(t),$(t),latest)" "$(if $(e),$(e),prod)"

push: ## Push a locally built image to ACR (n=component, t=tag [latest]).
	$(info Push image to ACR [n=$(n), t=$(if $(t),$(t),latest)])
	@cd ./scripts; ./push.sh "$(n)" "$(if $(t),$(t),latest)"

pull: ## Pull an image from ACR to the local registry (n=component, t=tag [latest]).
	$(info Pull image from ACR [n=$(n), t=$(if $(t),$(t),latest)])
	@cd ./scripts; ./pull.sh "$(n)" "$(if $(t),$(t),latest)"

tag: ## Retag an image in ACR so both tags point at the same image (n=component, f=from tag [latest], t=to tag, r=where to tag {remote,local} [remote]).
	$(info Tag image [n=$(n), f=$(if $(f),$(f),latest), t=$(t), $(if $(r),$(r),remote)])
	@cd ./scripts; ./tag.sh "$(n)" "$(if $(f),$(f),latest)" "$(t)" "$(if $(r),$(r),remote)"

.PHONY: build push pull tag

.PHONY: local
