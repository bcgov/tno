#!/bin/bash

echo 'Enter the Staging Repository Id you want to close'
read -p 'Repository Id: cabcgovtno-' varStagingRepoId

if [ ! -z "$varStagingRepoId" ]
then
  cd dal-db
  mvn nexus-staging:close -DstagingRepositoryId="cabcgovtno-$varStagingRepoId"
fi
