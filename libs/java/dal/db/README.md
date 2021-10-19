# Database Data Access Layer (DAL)

This project was created with the following command.

```bash
mvn archetype:generate -DgroupId=ca.bc.gov.tno.dal.db -DartifactId=dal-db -DarchetypeArtifactId=maven-archetype-quickstart -DarchetypeVersion=1.4 -DinteractiveMode=false
```

## Local Repository Package

In order to use this package in another project it must be provided by a package repository.
Ideally we would deploy to Maven Central Repository, however for testing purposes we can use a local repository.
To complicate matters, if you are using Development Containers, the local repository on each container is not easily accessible.
This requires the following manual steps.

To build a jar file use the following command.

```bash
mvn clean compile assembly:single
```

To install the jar into a local repository.
We do this to create a local copy that we can then move into a destination projects local repository.

```bash
mvn install:install-file \
   -Dfile=target/dal-db-0.0.1-alpha.jar \
   -DgroupId=ca.bc.gov.tno \
   -DartifactId=dal-db \
   -Dversion=0.0.1-alpha \
   -Dpackaging=jar \
   -DgeneratePom=true \
   -DlocalRepositoryPath=/workspaces/tno/libs/java/dal/db/target/repository
```