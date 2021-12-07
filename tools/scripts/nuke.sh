#!/bin/bash

echo ""
echo "*****************************************************"
echo "Nuking local TNO environment and configuration"
echo "*****************************************************"
echo ""

echo 'This operation will delete your configuration files!'
echo 'It will first make backup copies by renaming them (i.e. backup.env)'
read -p 'Enter "y" to continue: ' varContinue

if [ "$varContinue" == "y" ]; then
  echo "Deleting backup configurations"
  find . -type f -name '*backup.env' -exec rm {} +
  find . -type f -name 'flyway.backup.conf' -exec rm {} +
  find . -type f -name 'maven-settings.backup.xml' -exec rm {} +

  echo "Creating backup configurations"
  find . -type f -name ".env" -exec sh -c 'cp {} `dirname {}`/` {} .env`backup.env' \;
  find . -type f -name "flyway.conf" -exec sh -c 'cp {} `dirname {}`/`basename {} .conf`.backup.conf' \;
  find . -type f -name "maven-settings.xml" -exec sh -c 'cp {} `dirname {}`/`basename {} .xml`.backup.xml' \;

  echo "Deleting original configurations"
  find . -type f -name '.env' -exec rm {} +
  find . -type f -name 'flyway.conf' -exec rm {} +
  find . -type f -name 'maven-settings.xml' -exec rm {} +
fi
