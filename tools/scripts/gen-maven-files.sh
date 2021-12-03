#!/bin/bash

echo ""
echo "*********************************"
echo "Setting up Development Containers"
echo "*********************************"

varOssrhUsername=$(grep -Po '<username>\K[^\<]+' ./libs/java/dal/db/.devcontainer/maven-settings.xml)
if [ -z "$varOssrhUsername" ]
then
    echo 'Enter your OSSRH Maven Central username'
    read -p 'Username: ' varOssrhUsername
else
    echo "OSSRH Maven Central username: $varOssrhUsername"
fi

varOssrhPassword=$(grep -Po '<password>\K[^\<]+' ./libs/java/dal/db/.devcontainer/maven-settings.xml)
if [ -z "$varOssrhPassword" ]
then
    echo 'Enter your OSSRH Maven Central password.'
    read -p 'Password: ' varOssrhPassword
else
    echo "OSSRH Maven Central password: $varOssrhPassword"
fi


varDbUser=$(grep -Po 'POSTGRES_USER=\K.*$' ./db/postgres/.env)
varPassword=$(grep -Po 'POSTGRES_PASSWORD=\K.*$' ./db/postgres/.env)
varDbName=$(grep -Po 'POSTGRES_DB=\K.*$' ./db/postgres/.env)

###########################################################################
# TNO Configuration
###########################################################################

# Flyway configuration
if test -f "./libs/java/dal/db/dal-db-migration/flyway.conf"; then
    echo "./libs/java/dal/db/dal-db-migration/flyway.conf exists"
else
echo \
"flyway.user=$varDbUser
flyway.password=$varPassword
flyway.url=jdbc:postgresql://host.docker.internal:50002/$varDbName
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
      <username>$varOssrhUsername</username>
      <password>$varOssrhPassword</password>
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
      <username>$varOssrhUsername</username>
      <password>$varOssrhPassword</password>
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
      <username>$varOssrhUsername</username>
      <password>$varOssrhPassword</password>
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
      <username>$varOssrhUsername</username>
      <password>$varOssrhPassword</password>
    </server>
  </servers>
</settings>" >> ./libs/java/service/.devcontainer/maven-settings.xml
    echo "./libs/java/service/.devcontainer/maven-settings.xml created"
fi
