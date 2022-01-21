#!/bin/bash

echo 'Enter the Staging Repository Id you want to close'
read -p 'Repository Id: cabcgovtno-' varStagingRepoId

if [ ! -z "$varStagingRepoId" ]
then
  cd dal-db
  export MAVEN_OPTS="--add-opens=java.base/java.util=ALL-UNNAMED --add-opens=java.base/java.lang.reflect=ALL-UNNAMED --add-opens=java.base/java.text=ALL-UNNAMED --add-opens=java.desktop/java.awt.font=ALL-UNNAMED"
  sudo env PATH="$PATH" mvn nexus-staging:close -DstagingRepositoryId="cabcgovtno-$varStagingRepoId" -s /home/vscode/.m2/settings.xml
fi
