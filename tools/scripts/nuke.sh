#!/bin/bash
echo ""
echo "*****************************************************"
echo "Nuking local TNO environment and configuration"
echo "*****************************************************"
echo ""
# Set the file extention used for backed up files.
fileExt=".nkbak"

# Get a list of all current environment & config files into an array.
# Make sure that all name patterns are specified in 'find' command.
configFiles=()
while IFS= read -r -d $'\0'; do
    configFiles+=("$REPLY")
done < <(find . -type f \( -name "*.env" -o -name "flyway.conf" -o -name "maven-settings.xml" \) -print0)

# Proceed only if there are 1 or more files found matching the pattern, else do nothing.
numFiles=${#configFiles[@]}
if [[ $numFiles > 0 ]]; then
  echo "$numFiles configuration file(s) found."
  echo ""
  echo "WARNING: This operation will back-up and delete all current configuration files."
  read -p "Enter 'y' to continue: " varContinue
  # Continue if requested
  if [ "$varContinue" == "y" ]; then
    currentDT=$(date +"%Y%m%d-%H%M%S")
    # Should we delete all existing backups before renaming current config files?
    echo ""
    read -p "(d/y)elete all existing backups?: " varDelete
    if [[ "$varDelete" == "d" || "$varDelete" == "y" ]]; then
      echo "Deleting all old configuration backups..."
      find . -type f -name "*$fileExt" -delete
    else
      echo "Keeping old configuration backups."
    fi

    # For every configuration file found, move (rename) the file
    echo ""
    for file in "${configFiles[@]}"; do
      echo "Moving '$file' -> '$file.$currentDT$fileExt'"
      mv "$file" "$file.$currentDT$fileExt"
    done 
    echo "" 
    echo "All configuration files moved.."
  else
    # User chose not move configuration files.
    echo ""
    echo "Exiting."
  fi
else
  # No configuration files found to move. There is nothing to do.
  echo "No configuration files found. Exiting."
fi
