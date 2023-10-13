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

OPTIONS=ut:o:n:
LONGOPTS=update,topic:,oldvalue:,newvalue:

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

update=false topic= oldvalue= newvalue=
# now enjoy the options in order and nicely split until we see --
while true; do
  case "$1" in
    -u|--update)
      update=true
      shift
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
echo "topic: $topic, oldvalue: $oldvalue, newvalue: $newvalue"
echo ----------------------------------------------------------------------------

# Make variables available scripts.
export topic
export oldvalue
export newvalue

#################################################
# Work
#################################################

# Replace the topic id with the new value.
cd /var/lib/kafka/data

# Find all files for the specified topic.
files=(${topic}-*)

for file in "${files[@]}"; do
  echo --------------------------------
  echo Partition $file
  cat $file/partition.metadata ; echo
  if [[ $update == true ]]; then
    echo Updating $file
    sed -i "s/$oldvalue/$newvalue/" $file/partition.metadata
  fi
done
