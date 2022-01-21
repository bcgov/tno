#!/bin/bash

export GPG_TTY=$(tty)
cd dal-db
export MAVEN_OPTS="--add-opens=java.base/java.util=ALL-UNNAMED --add-opens=java.base/java.lang.reflect=ALL-UNNAMED --add-opens=java.base/java.text=ALL-UNNAMED --add-opens=java.desktop/java.awt.font=ALL-UNNAMED"
sudo env PATH="$PATH" mvn clean deploy -P staging -e -s /home/vscode/.m2/settings.xml
echo "Go to Nexus Repository Manager to get the Staging Repository Id - https://s01.oss.sonatype.org/"
