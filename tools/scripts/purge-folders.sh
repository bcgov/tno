#!/bin/bash

# *********************************************************************************
# Delete folders that are older than the specified number of days.
# Folder convention requires folders to be named with the date (i.e. yyyy-mm-dd).
# arg1: Folder path to purge (i.e. /data/capture)
# arg2: Number of days to keep in storage (i.e 2 = Delete everything two days old)
# *********************************************************************************

delete_folder () {
  FOLDER=$(basename "$1")
  DELETE_S=$(($(date +%s) - (24*60*60*${2-0})))
  FOLDER_S=$(date -d "$FOLDER" +%s)

  if [ "$FOLDER_S" -le "$DELETE_S" ]; then
    echo "DELETE $1"
    rm -rf $1
  fi
}
export -f delete_folder

purge_folders () {
  echo "************************************"
  echo "Purging $1"
  echo "Deleting folders older than $2 days"

  find $1/*/*/ -type d -exec bash -c 'delete_folder "$0" $1' {} $2 \;

  echo "************************************"
}

# purge_folders "/data/capture" 2
# purge_folders "/data/clip" 2

purge_folders $1 $2
