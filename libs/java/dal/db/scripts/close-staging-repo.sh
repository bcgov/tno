#!/bin/bash

echo 'Enter the Staging Repository Id you want to close'
read -p 'Repository Id: ' varStagingRepoId

if [ -z "$varStagingRepoId" ]
then
  cd dal-db
  mvn nexus-staging:close -DstagingRepositoryId=$varStagingRepoId
fi
