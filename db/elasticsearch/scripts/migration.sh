#!/bin/bash

#################################################
# Arguments
#################################################

# More safety, by turning some bugs into errors.
# Without `errexit` you don’t need ! and can replace
# PIPESTATUS with a simple $?, but I don’t do that.
set -o errexit -o pipefail -o noclobber -o nounset

# -allow a command to fail with !’s side effect on errexit
# -use return value from ${PIPESTATUS[0]}, because ! hosed $?
! getopt --test > /dev/null
if [[ ${PIPESTATUS[0]} -ne 4 ]]; then
    echo 'I’m sorry, `getopt --test` failed in this environment.'
    exit 1
fi

OPTIONS=rin:u:p:h:
LONGOPTS=rollback,ignore,version:,user:,password:,url:

# -regarding ! and PIPESTATUS see above
# -temporarily store output to be able to check for errors
# -activate quoting/enhanced mode (e.g. by writing out “--options”)
# -pass arguments only via   -- "$@"   to separate them correctly
! PARSED=$(getopt --options=$OPTIONS --longoptions=$LONGOPTS --name "$0" -- "$@")
if [[ ${PIPESTATUS[0]} -ne 0 ]]; then
    # e.g. return value is 1
    #  then getopt has complained about wrong arguments to stdout
    exit 2
fi
# read getopt’s output this way to handle the quoting right:
eval set -- "$PARSED"

version=* rollback=false ignore=false user=${ELASTIC_USERNAME:-} password=${ELASTIC_PASSWORD:-} url=${ELASTIC_URL:-}
# now enjoy the options in order and nicely split until we see --
while true; do
  case "$1" in
    -r|--rollback)
      # Remove objects from elasticsearch
      rollback=true
      shift
      ;;
    -i|--ignore)
      # Ignore errors and keep going
      ignore=true
      shift
      ;;
    -n|--version)
      version="$2"
      shift 2
      ;;
    -u|--user)
      user="$2"
      shift 2
      ;;
    -p|--password)
      password="$2"
      shift 2
      ;;
    -h|--url)
      url="$2"
      shift 2
      ;;
    --)
      shift
      break
      ;;
    *)
      echo "Programming error"
      exit 3
      ;;
  esac
done

if [ -z "$user" ]; then
  user=$(grep -Po 'ELASTIC_USERNAME=\K.*$' ./db/elasticsearch/.env)
  if [ -z "$user" ]; then
      echo "Enter a username for Elasticsearch."
      read -p 'Username: ' user
  fi
fi

if [ -z "$password" ]; then
  password=$(grep -Po 'ELASTIC_PASSWORD=\K.*$' ./db/elasticsearch/.env)
  if [ -z "$password" ]; then
      echo "Enter the password for Elasticsearch."
      read -p 'Password: ' password
  fi
fi

if [ -z "$url" ]; then
  url=$(grep -Po 'ELASTIC_URL=\K.*$' ./services/elastic/.env)
  if [ -z "$url" ]; then
      echo "Enter the URL to Elasticsearch."
      read -p 'URL: ' url
  fi
fi

echo "version: $version, user: $user, rollback: $rollback, ignore: $ignore"

#################################################
# Work
#################################################

auth=$(echo -ne "$user:$password" | base64 --wrap 0)

deleteIndex () {
  local indexName=$(echo $1 | sed 's/.*-index-\([^ ]*\)\.json/\1/')
  echo "Deleting index: $indexName" >&2
  local response=$(curl -X DELETE -H "Content-Type: application/json" -H "Authorization: Basic $auth" --write-out '%{http_code}' --silent --output /dev/null $url/$indexName)
  echo $response
}

addIndex() {
  local indexName=$(echo $1 | sed 's/.*-index-\([^ ]*\)\.json/\1/')
  echo "Adding index: $indexName" >&2
  local response=$(curl -X PUT -H "Content-Type: application/json" -H "Authorization: Basic $auth" -d @$fileName --write-out '%{http_code}' --silent --output /dev/null $url/$indexName)
  echo $response
}

# Rollback goes in reverse order
# Assumption is we only support
if [ "$rollback" = true ]; then
  for dir in $(find ./db/elasticsearch/migrations/$version/ -type d | sort -r); do
    for fileName in $(find $dir -type f -name *-index-*\.json | sort -r)
    do
      action=$(echo $fileName | sed 's/.*-\([cud]\)-index-.*\.json/\1/')
      case "$action" in
        c)
          response=$(deleteIndex $fileName)
          ;;
        u)
          # Reapply this migrations version of the index
          response=$(addIndex $fileName)
          ;;
        d)
          # No need to delete the object again
          ;;
        *)
          echo "Action identification missing on filename [cud]: $fileName"
          exit 3
          ;;
      esac

      if [ $response -lt 200 -o $response -gt 201 ]; then
        echo "Failed to delete index: $fileName"
        if [ $ignore = false ]; then
          exit 2
        fi
      fi
    done
  done
else
  for dir in $(find ./db/elasticsearch/migrations/$version/ -type d); do
    for fileName in $(find $dir -type f -name *-index-*\.json)
    do
      action=$(echo $fileName | sed 's/.*-\([cud]\)-index-.*\.json/\1/')
      case "$action" in
        c)
          response=$(addIndex $fileName)
          ;;
        u)
          response=$(addIndex $fileName)
          ;;
        d)
          response=$(deleteIndex $fileName)
          ;;
        *)
          echo "Action identification missing on filename [cud]: $fileName"
          exit 3
          ;;
      esac

      if [ $response -lt 200 -o $response -gt 201 ]; then
        echo "Failed to add index: $fileName"
        if [ $ignore = false ]; then
          exit 2
        fi
      fi
    done
  done
fi

