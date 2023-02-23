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

OPTIONS=ris:v:u:p:h:
LONGOPTS=rollback,ignore,step:,version:,user:,password:,url:

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

version=* rollback=false ignore=false user=${ELASTIC_USERNAME:-} password=${ELASTIC_PASSWORD:-} url=${ELASTIC_URL:-} step=""
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
    -v|--version)
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
    -s|--step)
      step="$2"
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
  port=$(grep -Po 'ELASTIC_HTTP_PORT=\K.*$' .env)
  url="http://host.docker.internal:$port"
  if [ -z "$port" ]; then
      echo "Enter the URL to Elasticsearch."
      read -p 'URL: ' url
  fi
fi

echo "version: $version, user: $user, rollback: $rollback, ignore: $ignore, url: $url"

#################################################
# Work
#################################################

auth=$(echo -ne "$user:$password" | base64 --wrap 0)

deleteIndex () {
  local fileName=$1
  local indexName=$(echo $1 | sed 's/.*\/[0-9]\+-d-index-\([^-]\+\)\.json$/\1/')
  echo "Deleting index: $indexName, File: $fileName" >&2
  local response=$(curl -X DELETE -H "Content-Type: application/json" -H "Authorization: Basic $auth" --write-out '%{http_code}' --silent --output /dev/null $url/$indexName)
  echo $response
}

addIndex() {
  local fileName=$1
  local indexName=$(echo $1 | sed 's/.*\/[0-9]\+-c-index-\([^-]\+\)\.json$/\1/')
  echo "Adding index: $indexName, File: $fileName" >&2
  local response=$(curl -X PUT -H "Content-Type: application/json" -v -u $user:$password -d @$fileName --write-out '%{http_code}' --silent --output /dev/null $url/$indexName)
  echo $response
}

updateIndex() {
  local fileName=$1
  local indexName=$(echo $1 | sed 's/.*\/[0-9]\+-u-index-\([^-]\+\)\.json$/\1/')
  echo "Updating index: $indexName, File: $fileName" >&2
  local response=$(curl -X PUT -H "Content-Type: application/json" -v -u $user:$password -d @$fileName --write-out '%{http_code}' --silent --output /dev/null $url/$indexName/_mapping)
  echo $response
}

reIndex() {
  local fileName=$1
  local indexName=$(echo $1 | sed 's/.*\/[0-9]\+-r-index-\([^-]\+\)\.json$/\1/')
  echo "Reindex: $indexName, File: $fileName" >&2
  local response=$(curl -X POST -H "Content-Type: application/json" -v -u $user:$password -d @$fileName --write-out '%{http_code}' --silent --output /dev/null $url/_reindex)
  echo $response
}

deleteAlias () {
  local fileName=$1
  local indexName=$(echo $1 | sed 's/.*\/[0-9]\+-d-alias-\([^-]\+\)-.\+\.json$/\1/')
  local aliasName=$(echo $1 | sed 's/.*\/[0-9]\+-d-alias-.\+-\([^ ]\+\)\.json$/\1/')
  echo "Deleting alias: $aliasName from $indexName, File: $fileName" >&2
  local response=$(curl -X DELETE -H "Content-Type: application/json" -H "Authorization: Basic $auth" --write-out '%{http_code}' --silent --output /dev/null $url/$indexName/_alias/$aliasName)
  echo $response
}

addAlias() {
  local fileName=$1
  local indexName=$(echo $1 | sed 's/.*\/[0-9]\+-c-alias-\([^-]\+\)-.\+\.json$/\1/')
  local aliasName=$(echo $1 | sed 's/.*\/[0-9]\+-c-alias-.\+-\([^ ]\+\)\.json$/\1/')
  echo "Adding alias: $aliasName to $indexName, File: $fileName" >&2
  local response=$(curl -X PUT -H "Content-Type: application/json" -v -u $user:$password --write-out '%{http_code}' --silent --output /dev/null $url/$indexName/_alias/$aliasName)
  echo $response
}

updateAlias() {
  local fileName=$1
  local indexName=$(echo $1 | sed 's/.*\/[0-9]\+-u-alias-\([^-]\+\)-.\+\.json$/\1/')
  local aliasName=$(echo $1 | sed 's/.*\/[0-9]\+-u-alias-.\+-\([^ ]\+\)\.json$/\1/')
  echo "Updating alias: $aliasName for $indexName, File: $fileName" >&2
  local response=$(curl -X POST -H "Content-Type: application/json" -v -u $user:$password -d @$fileName --write-out '%{http_code}' --silent --output /dev/null $url/$indexName/_alias/$aliasName)
  echo $response
}

path="up/"
if [ "$rollback" = true ]; then
  path="down/"
fi

for dir in $(find ./db/elasticsearch/migrations/$version/$path -type d); do
  for fileName in $(find $dir -type f -name *\.json | sort -z | xargs -r0 )
  do
    fileStep=$(echo $fileName | sed 's/.*\/\([0-9]\+\)-[crud]-.\+\.json$/\1/')
    action=$(echo $fileName | sed 's/.*\/[0-9]\+-\([crud]\)-.\+\.json$/\1/')
    object=$(echo $fileName | sed 's/.*\/[0-9]\+-[crud]-\([^-]\+\)-.\+\.json$/\1/')

    if [[ ! -z "$step" ]] && [ "$step" != "$fileStep" ]; then
      echo "Skip action: $action, Object: $object, Filename: $fileName"
      continue
    fi

    if [ "$object" == "index" ]; then
      case "$action" in
        c)
          response=$(addIndex $fileName)
          ;;
        u)
          response=$(updateIndex $fileName)
          ;;
        d)
          response=$(deleteIndex $fileName)
          ;;
        r)
          response=$(reIndex $fileName)
          ;;
        *)
          echo "Index action missing on filename [crud]: $fileName"
          exit 3
          ;;
      esac

      if [ $response -lt 200 -o $response -gt 201 ]; then
        echo "Response: $response failed to run index migration: $fileName"
        if [ $ignore = false ]; then
          exit 2
        fi
      fi
    elif [ "$object" == "alias" ]; then
      case "$action" in
        c)
          response=$(addAlias $fileName)
          ;;
        u)
          response=$(updateAlias $fileName)
          ;;
        d)
          response=$(deleteAlias $fileName)
          ;;
        *)
          echo "Alias action missing on filename [cud]: $fileName"
          exit 3
          ;;
      esac

      if [ $response -lt 200 -o $response -gt 201 ]; then
        echo "Response: $response failed to run index migration: $fileName"
        if [ $ignore = false ]; then
          exit 2
        fi
      fi
    fi
  done
done

