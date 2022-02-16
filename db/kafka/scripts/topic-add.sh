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

OPTIONS=e:t:b:p:r:i:
LONGOPTS=environment:,topic:,bootstrap:,partitions:,replication:,index:

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

# Default values.
environment=local bootstrap=${BOOTSTRAP:-tno-kafka-broker:29094} partitions=3 replication=3 index=0

# now enjoy the options in order and nicely split until we see --
while true; do
  case "$1" in
    -e|--environment)
      environment="$2"
      shift 2
      ;;
    -t|--topic)
      topic="$2"
      shift 2
      ;;
    -b|--bootstrap)
      bootstrap="$2"
      shift 2
      ;;
    -p|--partitions)
      partitions="$2"
      shift 2
      ;;
    -r|--replication)
      replication="$2"
      shift 2
      ;;
    -i|--index)
      index="$2"
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

if [ "$environment" = "local" ]; then
  bootstrap=${BOOTSTRAP:-broker:29092} partitions=1 replication=1
fi

if [ -z "$topic" ]; then
    echo "Enter a topic name."
    read -p 'Topic: ' topic
fi

if [ -z "$bootstrap" ]; then
    echo "Enter the host and port to the bootstrap server."
    read -p 'Host and Port: ' bootstrap
fi

echo "environment: $environment, topic: $topic, bootstrap: $bootstrap, partitions: $partitions, replication: $replication, index: $index"

# Make variables available scripts.
export environment
export topic
export bootstrap
export partitions
export replication
export index

#################################################
# Work
#################################################

if [ "$environment" = "local" ]; then
  docker exec -i tno-broker bash -c "/bin/kafka-topics --create --topic $topic --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"
else
  oc rsh -n 9b301c-$environment tno-kafka-broker-$index bash -c "/bin/kafka-topics --create --topic $topic --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"
fi
