# Postgres

Connect to remote database.

```bash
# Connect to remote Azure database.
psql -h mmi-prod.postgres.database.azure.com -p 5432 -U mmi mmi-prod

# Connect to Openshift database within Openshift
psql -h crunchy-primary -p 5432 -U admin tno
```

Create a backup.

```bash
# Create a backup of the production database by a pod in Openshift.
PGPASSWORD=""
pg_dump --column-inserts -d tno -h crunchy-primary -p 5432 -U admin -W -v > full-backup.sql
```

Restore the backup.

```bash
# Run SQL and restore the database.
psql -h postgres -p 5432 -U admin -f full-backup.sql -d tno
psql -h localhost -p 5432 -U postgres -f full-backup.sql -d tno
psql -h localhost -p 5432 -U admin -f full-backup.sql -d tno
psql -h mmi-prod.postgres.database.azure.com -p 5432 -U mmi -f full-backup.sql -d mmi-prod
```

```bash
#!/bin/bash
OUTPUT_FILE="./trace.out"

nohup psql -h localhost -p 5432 -U admin -d tno -a -f full-backup.sql >> ${OUTPUT_FILE} 2>&1 < /dev/null &
```

```bash
#!/bin/sh
while true
do
  > trace.out
  sleep 10m
done
```

```bash
nohup ./clear.sh 2>&1 < /dev/null &
```
