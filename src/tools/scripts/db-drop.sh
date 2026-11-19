#!/bin/bash

"${CONTAINER_ENGINE:-docker}" exec -i tno-database bash -c "psql -U \$POSTGRES_USER \$POSTGRES_DB -c 'DROP SCHEMA public CASCADE; CREATE SCHEMA public;'"
