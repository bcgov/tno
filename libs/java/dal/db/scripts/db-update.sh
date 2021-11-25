#!/bin/bash

cd dal-db-migration
mvn clean package
mvn clean flyway:migrate -Dflyway.configFiles=flyway.conf