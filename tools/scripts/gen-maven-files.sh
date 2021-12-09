#!/bin/bash

echo ""
echo "*********************************"
echo "Setting up Maven Configuration"
echo "*********************************"

. ./tools/scripts/variables.sh

ossrhUsername=$(grep -Po '<username>\K[^\<]+' ./libs/java/dal/db/.devcontainer/maven-settings.xml 2>/dev/null)
if [ -z "$ossrhUsername" ]
then
    echo 'Enter your OSSRH Maven Central username'
    read -p 'Username: ' ossrhUsername
else
    echo "OSSRH Maven Central username: $ossrhUsername"
fi

ossrhPassword=$(grep -Po '<password>\K[^\<]+' ./libs/java/dal/db/.devcontainer/maven-settings.xml 2>/dev/null)
if [ -z "$ossrhPassword" ]
then
    echo 'Enter your OSSRH Maven Central password.'
    read -p 'Password: ' ossrhPassword
else
    echo "OSSRH Maven Central password: $ossrhPassword"
fi

dbUser=$(grep -Po 'POSTGRES_USER=\K.*$' ./db/postgres/docker/.env)
password=$(grep -Po 'POSTGRES_PASSWORD=\K.*$' ./db/postgres/docker/.env)
dbName=$(grep -Po 'POSTGRES_DB=\K.*$' ./db/postgres/docker/.env)

###########################################################################
# Project Configuration
###########################################################################

# Flyway configuration
if test -f "./libs/java/dal/db/dal-db-migration/flyway.conf"; then
    echo "./libs/java/dal/db/dal-db-migration/flyway.conf exists"
else
echo \
"flyway.user=$dbUser
flyway.password=$password
flyway.url=jdbc:postgresql://host.docker.internal:$portDatabase/$dbName
flyway.schemas=public
flyway.baselineOnMigrate=true
# flyway.locations=filesystem:db/migration" >> ./libs/java/dal/db/dal-db-migration/flyway.conf
    echo "./libs/java/dal/db/dal-db-migration/flyway.conf created"
fi

# DAL DB Development Container Maven Settings
if test -f "./libs/java/dal/db/.devcontainer/maven-settings.xml"; then
    echo "./libs/java/dal/db/.devcontainer/maven-settings.xml exists"
else
echo \
"<?xml version=\"1.0\" encoding=\"UTF-8\"?>
<settings>
  <localRepository>\${user.home}/.m2/repository</localRepository>
  <interactiveMode>true</interactiveMode>
  <offline>false</offline>
  <servers>
    <server>
      <id>ossrh</id>
      <username>$ossrhUsername</username>
      <password>$ossrhPassword</password>
    </server>
  </servers>
</settings>" >> ./libs/java/dal/db/.devcontainer/maven-settings.xml
    echo "./libs/java/dal/db/.devcontainer/maven-settings.xml created"
fi

# DAL Elastic Development Container Maven Settings
if test -f "./libs/java/dal/elastic/.devcontainer/maven-settings.xml"; then
    echo "./libs/java/dal/elastic/.devcontainer/maven-settings.xml exists"
else
echo \
"<?xml version=\"1.0\" encoding=\"UTF-8\"?>
<settings>
  <localRepository>\${user.home}/.m2/repository</localRepository>
  <interactiveMode>true</interactiveMode>
  <offline>false</offline>
  <servers>
    <server>
      <id>ossrh</id>
      <username>$ossrhUsername</username>
      <password>$ossrhPassword</password>
    </server>
  </servers>
</settings>" >> ./libs/java/dal/elastic/.devcontainer/maven-settings.xml
    echo "./libs/java/dal/elastic/.devcontainer/maven-settings.xml created"
fi

# Core Development Container Maven Settings
if test -f "./libs/java/core/.devcontainer/maven-settings.xml"; then
    echo "./libs/java/core/.devcontainer/maven-settings.xml exists"
else
echo \
"<?xml version=\"1.0\" encoding=\"UTF-8\"?>
<settings>
  <localRepository>\${user.home}/.m2/repository</localRepository>
  <interactiveMode>true</interactiveMode>
  <offline>false</offline>
  <servers>
    <server>
      <id>ossrh</id>
      <username>$ossrhUsername</username>
      <password>$ossrhPassword</password>
    </server>
  </servers>
</settings>" >> ./libs/java/core/.devcontainer/maven-settings.xml
    echo "./libs/java/core/.devcontainer/maven-settings.xml created"
fi

# Service Development Container Maven Settings
if test -f "./libs/java/service/.devcontainer/maven-settings.xml"; then
    echo "./libs/java/service/.devcontainer/maven-settings.xml exists"
else
echo \
"<?xml version=\"1.0\" encoding=\"UTF-8\"?>
<settings>
  <localRepository>\${user.home}/.m2/repository</localRepository>
  <interactiveMode>true</interactiveMode>
  <offline>false</offline>
  <servers>
    <server>
      <id>ossrh</id>
      <username>$ossrhUsername</username>
      <password>$ossrhPassword</password>
    </server>
  </servers>
</settings>" >> ./libs/java/service/.devcontainer/maven-settings.xml
    echo "./libs/java/service/.devcontainer/maven-settings.xml created"
fi
