#!/bin/bash

docker exec -it tno-database bash -c "psql -U \$POSTGRES_USER \$POSTGRES_DB -c 'DROP SCHEMA public CASCADE; CREATE SCHEMA public;'"
