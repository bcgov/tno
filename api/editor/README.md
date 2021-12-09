# API Editor

To manually run the API.

```bash
mvn spring-boot:run
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

## Maven Central Repository

When a new package has recently been pushed to a remote repository it is likely your local cached information is out-of-date.
By default maven will not make a request for new information until cache expires.
You can force the cache to be updated with the `-U` argument.

```bash
# Or clear the local repository
mvn dependency:purge-local-repository

# Force maven to fetch dependencies from remote repository.
mvn clean install -U -X
```

To view dependency sources.

```bash
# List dependency sources.
mvn dependency:sources

# Clear local cache.
mvn dependency:purge-local-repository -DmanualInclude="groupId:artifactId"
```
