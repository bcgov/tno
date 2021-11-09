#!/bin/bash

echo 'Enter the Staging Repository Id you want to release'
read -p 'Repository Id: cabcgovtno-' varStagingRepoId

if [ ! -z "$varStagingRepoId" ]
then
  cd dal-db
  mvn nexus-staging:release -P staging -DstagingRepositoryId="cabcgovtno-$varStagingRepoId"
fi
