#!/bin/bash

echo ""
echo "*****************************************************"
echo "Nuking local TNO environment and configuration"
echo "*****************************************************"
echo ""

echo 'This operation will delete your configuration files!'
read -p 'Enter "y" to continue: ' varContinue

if [ "$varContinue" == "y" ]; then
  find . -type f -name '*.env' -exec rm {} +
  find . -type f -name 'flyway.conf' -exec rm {} +
  find . -type f -name 'maven-settings.xml' -exec rm {} +
fi
