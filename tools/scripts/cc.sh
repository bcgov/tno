#!/bin/bash

#########################################
# Capture Closed Caption
#
# Runs command to listen to the /dev/vbi# device
# and captures the text to the specified file.
# Inserts timestamp every 10 seconds.
#
# Arguments
# $1 = output file (default: cc.txt)
# $2 = device to listen to (default: /dev/vbi0)
#########################################

output=${$1:-"cc.txt"}
device=${$2:-"/dev/vbi0"}

echo "% $(date +%F_%H:%M:%S)" >| $output
zvbi-ntsc-cc -d $device -cxvp >> $output &

NTSC=$!

while true
do
  read -t 10 -n 1 keypress
  if [[ $keypress = "q" ]] || [[ $keypress = "Q" ]]; then
    echo
    break
  fi
    echo "% $(date +%F_%H:%M:%S)" >> $output
done

kill -9 $NTSC
