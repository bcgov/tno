# Database Data Access Layer (DAL)

This project was created with the following command.

```bash
mvn archetype:generate -DgroupId=ca.bc.gov.tno.dal.db -DartifactId=dal-db -DarchetypeArtifactId=maven-archetype-quickstart -DarchetypeVersion=1.4 -DinteractiveMode=false
```

## Local Repository Package

In order to use this package in another project it must be provided by a package repository.
If you use a local repository you can share the packages between projects.
However, if you are using Development Containers you must copy the packages manually between containers.

To build a jar file use the following command.

```bash
mvn clean compile assembly:single
```

To install the jar into a local repository.
We do this to create a local copy that we can then move into a destination projects local repository.

```bash
mvn install:install-file \
   -Dfile=target/dal-db-0.0.1-SNAPSHOT.jar \
   -DgroupId=ca.bc.gov.tno \
   -DartifactId=dal-db \
   -Dversion=0.0.1-SNAPSHOT \
   -Dpackaging=jar \
   -DgeneratePom=true \
   -DlocalRepositoryPath=/workspaces/tno/libs/java/dal/db/target/repository
```

## Maven Central Repository

To push an image to Maven Central you must have an account setup.
Tutorial example provided [here](https://dzone.com/articles/how-to-create-a-java-library-from-scratch-to-maven).

Information

- [Central Sonatype](https://central.sonatype.org/publish/publish-maven/)
- [Tutorial](https://dzone.com/articles/how-to-create-a-java-library-from-scratch-to-maven)
- [Nexus Repository Manager](https://s01.oss.sonatype.org/index.html#view-repositories;releases~browsestorage)
- [Maven Release Plugin](https://axelfontaine.com/blog/maven-releases-steroids-2.html)

First make sure your development environment has GPG.
More information [here](https://central.sonatype.org/publish/requirements/gpg/).
You will need to publish your public keys to a signing server for your packages to be validated.

```bash
# Fill out information to generate a key.
gpg --gen-key

# If you run into issues with the maven plugin this may resolve it.
export GPG_TTY=$(tty)
```

For some reason with the latest package versions the Maven plugin doesn't work correctly.
To fix this issue run the following command.

```bash
export JDK_JAVA_OPTIONS='--add-opens java.base/java.util=ALL-UNNAMED --add-opens java.base/java.lang.reflect=ALL-UNNAMED --add-opens java.base/java.text=ALL-UNNAMED --add-opens java.desktop/java.awt.font=ALL-UNNAMED'
```

Package and deploy to Maven Central.

```bash
mvn clean deploy -P staging
```

You package should now be visible in the Maven Central Staging Repositories [https://s01.oss.sonatype.org/](https://s01.oss.sonatype.org/).
Obtain the package Id from the website.

Close the staging repository before you can release it.

```bash
mvn nexus-staging:close -DstagingRepositoryId={Staging Repository Id}
```

Once the repository is closed you can release.

```bash
mvn nexus-staging:release -DstagingRepositoryId={Staging Repository ID}
```

## GPG Keys

If you need to access your signing keys, share them or upload them to a signing server.

```bash
# Export the public key to a file.  Replace the curly brackets with your uid (probably email).
gpg --export -a "{uid}" > public.key

# Get your public key.
gpg --list-keys

# Distribute your public key.  Replace curly brackets with your public key.
gpg --keyserver keyserver.ubuntu.com --send-keys {public key}

# Export private keys to share with other computers.  Replace the curly brackets with your uid (probably email).
# Do not commit your keys to source control.
gpg --export-secret-keys "{uid}" > private.key

# Import the private key on another computer.
gpg --import private.key
```

## Database Migration

They Flyway package is used to run database migrations.
The free version does not support rollbacks regrettably.

Create a configuration file here `./api-editor/flyway.conf`.

Populate that file with the following.
Replace the curly bracket values.

```conf
flyway.user={username}
flyway.password={password}
flyway.url={url:jdbc:postgresql://host.docker.internal:40000/tno}
flyway.schemas={schema:public}
flyway.baselineOnMigrate=true
# flyway.locations=filesystem:db/migration
```

To run a database migration.

```bash
mvn clean flyway:migrate -Dflyway.configFiles=flyway.conf
```
