#!/bin/bash
set -e

psql -U postgres -v ON_ERROR_STOP=1 <<-EOSQL
    CREATE DATABASE $KEYCLOAK_DATABASE;
EOSQL
