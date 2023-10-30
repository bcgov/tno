#!/bin/bash

echo "Drop and refresh keycloak database"
. ./tools/scripts/variables.sh
docker exec -i tno-database bash -c "psql -U \$POSTGRES_USER \$POSTGRES_DB -c 'DROP DATABASE IF EXISTS $keycloakDbName;' -c 'CREATE DATABASE $keycloakDbName;';"

