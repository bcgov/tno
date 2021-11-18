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

###########################################################################
# TNO Configuration
###########################################################################

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

# Flyway configuration
if test -f "./libs/java/dal/db/flyway.conf"; then
    echo "./libs/java/dal/db/flyway.conf exists"
else
echo \
"flyway.user=admin
flyway.password=lka32%alskdjf!9987_A!
flyway.url=jdbc:postgresql://host.docker.internal:50002/tno
flyway.schemas=public
flyway.baselineOnMigrate=true
# flyway.locations=filesystem:db/migration" >> ./libs/java/dal/db/flyway.conf
    echo "./libs/java/dal/db/flyway.conf created"
fi
