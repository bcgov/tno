#!/bin/bash

export GPG_TTY=$(tty)
cd dal-db
mvn clean deploy -P release -e
echo "Go to Nexus Repository Manager to get the Staging Repository Id - https://s01.oss.sonatype.org/"