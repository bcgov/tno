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

OPTIONS=p:b:
LONGOPTS=partitions:,bootstrap:

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

partitions=6 bootstrap=kafka-broker-0.kafka-headless:9092,kafka-broker-1.kafka-headless:9092,kafka-broker-2.kafka-headless:9092,kafka-broker-3.kafka-headless:9092
# now enjoy the options in order and nicely split until we see --
while true; do
  case "$1" in
    -p|--partitions)
      partitions="$2"
      shift 2
      ;;
    -b|--bootstrap)
      bootstrap="$2"
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

if [ -z "$bootstrap" ]; then
    echo "Enter the host and port to the bootstrap server."
    read -p 'Host and Port: ' bootstrap
fi

echo "partitions: $partitions, bootstrap: $bootstrap"

#################################################
# Work
#################################################

# BOOTSTRAP=kafka-broker-0.kafka-headless:9092,kafka-broker-1.kafka-headless:9092,kafka-broker-2.kafka-headless:9092,kafka-broker-3.kafka-headless:9092
# PARTITIONS=6

# List topics
TOPICS=( $(kafka-topics --bootstrap-server $bootstrap --list) )

for TOPIC in "${TOPICS[@]}"
do
  if [[ $TOPIC != "__consumer_offsets" ]]; then
    echo $TOPIC
    kafka-topics --bootstrap-server $bootstrap --alter --topic $TOPIC --partitions $partitions
  fi
done
