#!/bin/bash

cd dal-db-migration
mvn clean package
mvn flyway:migrate -Dflyway.configFiles=flyway.conf