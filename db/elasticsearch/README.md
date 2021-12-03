# Elasticsearch

Elasticsearch provides a non-relational database to enable performant indexing and search.

More information [here](https://www.elastic.co/).

## Migrations

To provide automation to migrations a script has been created which will enable applying versions and rolling back versions.
This script has a number of naming conventions that must be followed for it to work.

Note that when a failure occurs, by default the script will abort immediately.
However this does not resolve any prior successful actions.
Which means you can get into a state where it's impossible to rerun without ignoring errors.

### Conventions

1. Migrations are stored in the `./db/elasticsearch/migrations/` folder
2. Each migration name must be sorted alphabetically if they are to be applied in order. This means `01.00.00` is a better format than `1.0.0` because `12.0.0` would come before `2.0.0`.
3. Each file in the migration version folder must be `json`
4. Each file also may need to be sorted alphabetically
5. Each file has a naming convention `$(order)-$(action)-$(type)-$(name).json`. The **action** must be one of the following `c` (create) or `u` (update) or `d` (delete). Currently the only supported **type** is `index`

### Arguments

| Argument       | Default                                   | Description                                                                     |
| -------------- | ----------------------------------------- | ------------------------------------------------------------------------------- |
| -n\|--version  | \*                                        | Which migration version to run                                                  |
| -u\|--user     | Extract from `.env` or `ELASTIC_USERNAME` | The elastic username                                                            |
| -p\|--password | Extract from `.env` or `ELASTIC_PASSWORD` | The elastic user password                                                       |
| -r\|--rollback | false                                     | Rollback the migration(s)                                                       |
| -i\|--ignore   | false                                     | Whether to ignore errors and continue execution. Default is to exit immediately |

You can run the script directly from root only presently.

```bash
# Apply all migrations
./db/elasticsearch/scripts/migration.sh -u john -p password

# Apply only the 1.0.0 migration
./db/elasticsearch/scripts/migration.sh -n 1.0.0 -u john -p password

# Rollback all migrations and ignore errors
./db/elasticsearch/scripts/migration.sh -r -i -u john -p password

# Rollback only the 1.0.0 migrations
./db/elasticsearch/scripts/migration.sh -n 1.0.0 -r -u john -p password
```
