#!/bin/bash

varKeycloak=$(grep -Po 'KEYCLOAK_USER=\K.*$' ./auth/keycloak/.env)
if [ -z "$varKeycloak" ]
then
    echo 'Enter a username for the keycloak realm administrator'
    read -p 'Username: ' varKeycloak
else
    echo "Your keycloak username: $varKeycloak"
fi

varDbUser=$(grep -Po 'POSTGRES_USER=\K.*$' ./db/postgres/docker/.env)
if [ -z "$varDbUser" ]
then
    echo 'Enter a username for the Postgres database.'
    read -p 'Username: ' varDbUser
else
    echo "Your database username: $varDbUser"
fi

varElastic=$(grep -Po 'ELASTIC_USERNAME=\K.*$' ./db/elasticsearch/.env)
if [ -z "$varElastic" ]
then
    echo 'Enter a username for Elasticsearch.'
    read -p 'Username: ' varElastic
else
    echo "Your Elasticsearch username: $varElastic"
fi

varAzureCognitiveServiceKey=$(grep -Po 'COGNITIVE_SERVICES_SPEECH_SUBSCRIPTION_KEY=\K.*$' ./api/editor/.env)
if [ -z "$varAzureCognitiveServiceKey" ]
then
    echo 'Enter your Azure Cognitive Service subscription key.'
    read -p 'Key: ' varAzureCognitiveServiceKey
else
    echo "Your Azure Cognitive Service subscription key: $varAzureCognitiveServiceKey"
fi

varAzureCognitiveServiceRegion=$(grep -Po 'COGNITIVE_SERVICES_SPEECH_REGION=\K.*$' ./api/editor/.env)
if [ -z "$varAzureCognitiveServiceRegion" ]
then
    echo 'Enter your Azure Cognitive Service region (i.e. canadacentral).'
    read -p 'Region: ' varAzureCognitiveServiceRegion
else
    echo "Your Azure Cognitive Service region: $varAzureCognitiveServiceRegion"
fi

varAzureVideoAnalyzerKey=$(grep -Po 'AZURE_VIDEO_ANALYZER_SUBSCRIPTION_KEY=\K.*$' ./api/editor/.env)
if [ -z "$varAzureVideoAnalyzerKey" ]
then
    echo 'Enter your Azure Video Analyzer subscription key.'
    read -p 'Key: ' varAzureVideoAnalyzerKey
else
    echo "Your Azure Video Analyzer subscription key: $varAzureVideoAnalyzerKey"
fi

varAzureVideoAccountId=$(grep -Po 'AZURE_VIDEO_ANALYZER_ACCOUNT_ID=\K.*$' ./api/editor/.env)
if [ -z "$varAzureVideoAccountId" ]
then
    echo 'Enter your Azure Video Analyzer account ID.'
    read -p 'Account ID: ' varAzureVideoAccountId
else
    echo "Your Azure Video Analyzer account ID: $varAzureVideoAccountId"
fi

varAzureVideoLocation=$(grep -Po 'AZURE_VIDEO_ANALYZER_LOCATION=\K.*$' ./api/editor/.env)
if [ -z "$varAzureVideoLocation" ]
then
    echo 'Enter your Azure Video Analyzer location (i.e. trial).'
    read -p 'Location: ' varAzureVideoLocation
else
    echo "Your Azure Video Analyzer location: $varAzureVideoLocation"
fi

# Only required if the Azurite docker container doesn't allow for local domain names.
# Workaround is to either use 'mcr.microsoft.com/azure-storage/azurite:3.14.0', or use the IP address.
# echo 'Enter the IP of your local host.docker.internal.'
# read -p 'IP: ' varHostDockerInternal

varPassword=$(grep -Po 'POSTGRES_PASSWORD=\K.*$' ./db/postgres/docker/.env)

if [ -z "$varPassword" ]
then
  # Generate a random password that satisfies password requirements.
  echo 'A password is randomly being generated.'
  varPassword=$(date +%s | sha256sum | base64 | head -c 29)A8!
  echo "Your generated password is: $varPassword"
else
  echo "Your password is: $varPassword"
fi

azureKey=$(date +%s | sha256sum | base64 | head -c 29)

varDbName="tno"
