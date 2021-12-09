#!/bin/bash

echo ""
echo "*****************************************************"
echo "Nuking local TNO environment and configuration"
echo "*****************************************************"
echo ""

# Get a list of all files into an array. Make sure all name patterns are specified in "find" command.
configFiles=()
while IFS= read -r -d $'\0'; do
    configFiles+=("$REPLY")
done < <(find . -type f \( -name "*.env" -o -name "flyway.conf" -o -name "maven-settings.xml" \) -print0)

# Proceed only if there are files found matching the pattern, else do nothing.
numFiles=${#configFiles[@]}
if [[ $numFiles > 0 ]]; then
  echo -e "$numFiles environment files found.\n"

  echo "This operation will delete your current configuration files!"
  echo "It will first make backup copies by renaming them (i.e. .env.yyymmdd-HHMMSS.bak)"
  read -p "Enter 'y' to continue: " varContinue

  if [ "$varContinue" == "y" ]; then
    currentDT=$(date +"%y%m%d-%H%M%S")
    
    #Ask user if they want to delete all existing backups before moving current config files
    echo -e "\nWould you like to delete any existing backup files before backing up the existing configuration?"
    read -p "Enter 'd' to delete existing backups first. Any other key to keep them: " varDelete
    if [ "$varDelete" == "d" ]; then
      echo "Deleting all old configuration backups."
      find . -type f -name "*.bak" -delete
    else
      echo "Keeping old configuration backups."
    fi
    echo ""
    # For every configuration file found, move (rename) the file
    for file in "${configFiles[@]}"; do
      echo "Moving '$file' to '$file.$currentDT.bak'"
      mv "$file" "$file.$currentDT.bak"
    done  
    echo -e "\nConfiguration files moved and are ready to be regenerated."
  else
    # User chose not move configuration files.
    echo -e "\nExiting with no action."
  fi
else
  # No configuration files found to move. There is nothing to do.
  echo "No environment files found. Exiting."
fi
