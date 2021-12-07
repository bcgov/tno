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

OPTIONS=rn:z:p:
LONGOPTS=rollback,version:,zookeeper:,partitions:,replication:

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

version=* rollback=false zookeeper=${ZOOKEEPER:-zookeeper:2181} partitions=1 replication=1
# now enjoy the options in order and nicely split until we see --
while true; do
  case "$1" in
    -r|--rollback)
      # Remove objects from kafka
      rollback=true
      shift
      ;;
    -n|--version)
      version="$2"
      shift 2
      ;;
    -z|--zookeeper)
      zookeeper="$2"
      shift 2
      ;;
    -p|--partitions)
      partitions="$2"
      shift 2
      ;;
    --replication)
      replication="$2"
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

if [ -z "$zookeeper" ]; then
    echo "Enter the host and port to Zookeeper."
    read -p 'Host and Port: ' zookeeper
fi

echo "version: $version, zookeeper: $zookeeper, rollback: $rollback, partitions: $partitions, replication: $replication"

# Make variables available scripts.
export zookeeper
export partitions
export replication

#################################################
# Work
#################################################

if [ "$rollback" = true ]; then
  for fileName in $(find ./db/kafka/migrations/U$version -type f); do
    echo "Execute $fileName"
    . $fileName
  done
else
  for fileName in $(find ./db/kafka/migrations/V$version -type f); do
    echo "Execute $fileName"
    . $fileName
  done
fi

