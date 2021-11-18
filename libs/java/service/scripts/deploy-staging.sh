#!/bin/bash

export GPG_TTY=$(tty)
export MAVEN_OPTS="--add-opens=java.base/java.util=ALL-UNNAMED --add-opens=java.base/java.lang.reflect=ALL-UNNAMED --add-opens=java.base/java.text=ALL-UNNAMED --add-opens=java.desktop/java.awt.font=ALL-UNNAMED"
mvn clean deploy -P staging -e
echo "Go to Nexus Repository Manager to get the Staging Repository Id - https://s01.oss.sonatype.org/"