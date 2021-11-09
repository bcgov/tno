#!/bin/bash

echo 'Enter the Staging Repository Id you want to drop'
read -p 'Repository Id: cabcgovtno-' varStagingRepoId

if [ ! -z "$varStagingRepoId" ]
then
  cd dal-db
  mvn nexus-staging:drop -P staging -DstagingRepositoryId="cabcgovtno-$varStagingRepoId"
fi
