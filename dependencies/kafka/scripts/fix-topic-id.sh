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

OPTIONS=up:t:o:n:
LONGOPTS=update,project:,topic:,oldvalue:,newvalue:

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

project=9b301c-dev update=false topic= oldvalue= newvalue=
# now enjoy the options in order and nicely split until we see --
while true; do
  case "$1" in
    -u|--update)
      update=true
      shift
      ;;
    -p|--project)
      project="$2"
      shift 2
      ;;
    -t|--topic)
      topic="$2"
      shift 2
      ;;
    -o|--oldvalue)
      oldvalue="$2"
      shift 2
      ;;
    -n|--newvalue)
      newvalue="$2"
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

if [ -z "$project" ]; then
    echo "Enter the Openshift project name."
    read -p 'Project name: ' project
fi

if [ -z "$topic" ]; then
    echo "Enter the topic name."
    read -p 'Topic name: ' topic
fi

if [[ -z "$oldvalue" ]] && [[ $update == true ]]; then
    echo "Enter the invalid topic ID."
    read -p 'Topic ID: ' oldvalue
fi

if [[ -z "$newvalue" ]] && [[ $update == true ]]; then
    echo "Enter the valid topic ID."
    read -p 'Topic ID: ' newvalue
fi

echo ----------------------------------------------------------------------------
echo "project: $project, topic: $topic, oldvalue: $oldvalue, newvalue: $newvalue"
echo ----------------------------------------------------------------------------

#################################################
# Work
#################################################

# Get all Kafka brokers
pods=($(oc get pods -n $project --no-headers -o custom-columns=POD:.metadata.name -l name=kafka-broker))

for pod in "${pods[@]}"; do
  # Execute script in pod to fix the topic id
  echo -----------------------------
  echo Connecting $pod
  if [[ $update == true ]]; then
    cat ./ssh/topic-id.sh | oc rsh -n $project $pod bash -s - -t $topic -o $oldvalue -n $newvalue -u
  else
    cat ./ssh/topic-id.sh | oc rsh -n $project $pod bash -s - -t $topic -o $oldvalue -n $newvalue
  fi
done
