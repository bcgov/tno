# Database Backup Service

Provides cron based backups using plugin developed for https://github.com/BCDevOps/backup-container.

This pod runs a chron job internally (does not use the Openshift default chron job features).
Default configuration is to run nightly.
Pod also provides a way to restore backups and verify them.

## Remote Shell to Containers

Using the `oc` CLI you can remote SSH into the backup pod.
This provides a way to manually run backup and restore commands.

```bash
oc rsh ${podName}
```

## Backup Script Commands

| Command                                                    | Param    | Description                    |
| ---------------------------------------------------------- | -------- | ------------------------------ |
| `./backup.sh -h`                                           | -h       | Display script documentation   |
| `./backup.sh -l`                                           | -l       | List existing backups          |
| `./backup.sh -c`                                           | -c       | List current configuration     |
| `./backup.sh -1`                                           | -1       | Run a single backup cycle      |
| `./backup.sh -r $databaseSpec [-f $backupFileFilter]`      | -r -f    | Restore a database from backup |
| `./backup.sh [-s] -v $databaseSpec [-f $backupFileFilter]` | -s -v -f | Verify backups                 |

**$databaseSpec** is the connection string to the database (i.e. `postgres=tno-database:5432/tno`).
Find this value with the `./backup.sh -c` command.

Example of a restore.

```bash
./backup.sh -r postgres=tno-database:5432/tno -f tno-database-tno_2021
```

If you want a single command to SSH and perform a backup use the following syntax.

```bash
oc rsh $(oc get pod -l name=tno-backup --no-headers -o custom-columns=POD:.metadata.name) ./backup.sh -l
```
