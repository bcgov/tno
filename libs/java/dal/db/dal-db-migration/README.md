# Flyway Database Migration

Build and run the image that will run the flyway database migration.
This will run the database migration for the current build.

```bash
# Build
docker build -t tno:db-migration.
# Run
docker run -itd --env-file=.env tno:db-migration
```
