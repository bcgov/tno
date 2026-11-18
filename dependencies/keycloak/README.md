# Keycloak

Open Source Identity and Access Management for Modern Applications and Services.

Add authentication to applications and secure services with minimum fuss. No need to deal with storing users or authenticating users. It's all available out of the box.

You'll even get advanced features such as User Federation, Identity Brokering and Social Login.

- [Keycloak](https://www.keycloak.org/)
- [Docker](https://hub.docker.com/r/jboss/keycloak/)
- [Github](https://github.com/keycloak/keycloak-containers/tree/master/server)

## Local browser-test accounts

The local `mmi` realm seeds `admin`, `editor` and `subscriber`, with password
`password`. Its user profile requires `display_name`, `idir_username` and
`idir_user_guid` in addition to email, first name and last name. The realm import
includes these attributes so browser login does not stop at **Update Account
Information**. These are synthetic local identities, not real IDIR accounts.

For an existing local realm imported before these attributes were added, open
`http://localhost:40001`, sign in to the admin console with `admin` / `password`,
select **mmi → Users**, and fill the missing attributes:

| User | Display Name (`display_name`) | IDIR (`idir_username`) | IDIR GUID (`idir_user_guid`) |
| --- | --- | --- | --- |
| editor | Editor User | EDITOR | eddba7fe-cbb9-4d26-804c-8208f43b5057 |
| subscriber | Subscriber User | SUBSCRIBER | 62de5f53-707b-4008-8b68-dff42de8c948 |

Preserve existing nonempty attributes, roles and passwords. Restarting the container
does not update an already imported realm; repairing these users in place avoids
recreating the database. No change to the realm's required-field rules is needed.

## Export Realm Configuration

After configuring Keycloak you can export the configuration to a JSON file so that it can be used to initialize a new Keycloak instance.
If you use the UI to export it will not contain all the necessary information and settings, thus the need for this CLI option.

More information [here](https://www.keycloak.org/docs/latest/server_admin/index.html#_export_import).

Once the keycloak container is running, ssh into it and execute the following commands;

### Find Docker Container ID

```bash
docker ps
```

### SSH into the Container

```bash
docker exec -it {ContainerID} bash
```

### Export the Configuration

```bash
docker exec -it tno-keycloak bash

# Old jboss image
/opt/jboss/keycloak/bin/standalone.sh \
  -Dkeycloak.migration.action=export \
  -Dkeycloak.migration.provider=singleFile \
  -Dkeycloak.migration.realmName=tno \
  -Dkeycloak.migration.file=/tmp/realm-export.json \
  -Dkeycloak.migration.usersExportStrategy=REALM_FILE \
  -Dkeycloak.migration.strategy=OVERWRITE_EXISTING \
  -Djboss.http.port=8888 \
  -Djboss.https.port=9999 \
  -Djboss.management.http.port=7777

# New Keycloak image
/opt/keycloak/bin/kc.sh \
  export --file /opt/keycloak/data/import/realm-export.json --realm tno
```

## Import Realm

If you have issues with the automatic import you can manually import the realm.
To import a previously exported realm configuration execute the following command;

```bash
docker exec -it keycloak bash

# Old jboss image
/opt/jboss/keycloak/bin/standalone.sh \
  -Djboss.socket.binding.port-offset=100 \
  -Dkeycloak.migration.action=import \
  -Dkeycloak.profile.feature.scripts=enabled \
  -Dkeycloak.profile.feature.upload_scripts=enabled \
  -Dkeycloak.migration.provider=singleFile \
  -Dkeycloak.migration.file=/tmp/realm-export.json

# New Keycloak image
/opt/keycloak/bin/kc.sh \
  import --file /opt/keycloak/data/import/realm-export.json
```

or

```bash
# Old jboss image
$ docker run -e KEYCLOAK_USER=<USERNAME> -e KEYCLOAK_PASSWORD=<PASSWORD> \
    -e KEYCLOAK_IMPORT=/tmp/example-realm.json -v /tmp/example-realm.json:/tmp/example-realm.json jboss/keycloak
```

### Manual Steps

Note that the import script has hardcoded passwords, which will override whatever password you have in your `.env`.

> user: keycloak, pwd: password

1. Stop Keycloak `$ docker-compose stop keycloak keycloak-db`
2. Remove containers `$ docker rm keycloak` and `$ docker rm keycloak-db`
3. Remove volume `$ docker volume rm tno_keycloak-db-data`
4. Build Keycloak `$ docker-compose build keycloak`
5. Make sure your `.env` comments out this line `# KEYCLOAK_IMPORT=/tmp/realm-export.json`
6. Start Keycloak `$ docker-compose up -d keycloak`
7. Open Keycloak in your browser http://keycloak:8080, verify the `master` realm is there. **It will take a minute to initialize.**
8. Bash into the Keycloak docker container `$ docker exec -it keycloak bash`
9. Run the import standalone.sh script **above**. It will hang on this line - `18:30:37,822 WARN [org.keycloak.common.Profile] (ServerService Thread Pool -- 68) Preview feature enabled: scripts`. **Let it run for a minute.**
10. Press `ctrl+c` and exit the `bash`
11. Restart Keycloak `$ docker-compose restart keycloak`
12. Open Keycloak in your browser http://keycloak:8080, verify the `tno` ream is there. **It will take a minute to initialize.**
