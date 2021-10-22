#!/bin/bash

echo 'Enter the Staging Repository Id you want to release'
read -p 'Repository Id: ' varStagingRepoId

if [ -z "$varStagingRepoId" ]
then
  cd dal-db
  mvn nexus-staging:release -DstagingRepositoryId=$varStagingRepoId
fi
