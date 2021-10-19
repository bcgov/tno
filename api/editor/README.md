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
flyway.url={url:jdbc:postgresql://host.docker.internal:50002/tno}
flyway.schemas={schema:public}
flyway.baselineOnMigrate=true
# flyway.locations=filesystem:db/migration
```

To run a database migration.

```bash
mvn clean flyway:migrate -Dflyway.configFiles=flyway.conf
```