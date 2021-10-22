#!/bin/bash

cd dal-db
mvn clean flyway:migrate -Dflyway.configFiles=flyway.conf