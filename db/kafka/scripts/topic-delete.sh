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

OPTIONS=t:b:
LONGOPTS=topic:,bootstrap:

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

bootstrap=${BOOTSTRAP:-broker:29092}
# now enjoy the options in order and nicely split until we see --
while true; do
  case "$1" in
    -t|--topic)
      topic="$2"
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

if [ -z "$topic" ]; then
    echo "Enter a topic name."
    read -p 'Topic: ' topic
fi

if [ -z "$bootstrap" ]; then
    echo "Enter the host and port to the bootstrap server."
    read -p 'Host and Port: ' bootstrap
fi

echo "topic: $topic, bootstrap: $bootstrap"

# Make variables available scripts.
export topic
export bootstrap

#################################################
# Work
#################################################

docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic $topic --bootstrap-server $bootstrap"
