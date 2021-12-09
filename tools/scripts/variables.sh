#!/bin/bash

######################################################################
# Keycloak configuration
######################################################################

export keycloakDbName="keycloak"

export keycloakUser=$(grep -Po 'KEYCLOAK_USER=\K.*$' ./auth/keycloak/.env 2>/dev/null)
if [ -z "$keycloakUser" ]
then
    echo 'Enter a username for the keycloak realm administrator'
    read -p 'Username: ' keycloakUser
    export keycloakUser
else
    echo "Your keycloak username: $keycloakUser"
fi

export keycloakPassword=$(grep -Po 'KEYCLOAK_USER=\K.*$' ./auth/keycloak/.env 2>/dev/null)
if [ -z "$keycloakPassword" ]
then
    echo 'Enter a password for the keycloak realm administrator'
    read -p 'Password: ' keycloakPassword
    export keycloakPassword
else
    echo "Your keycloak password: $keycloakPassword"
fi

######################################################################
# Database configuration
######################################################################

export dbName="tno"

export dbUser=$(grep -Po 'POSTGRES_USER=\K.*$' ./db/postgres/docker/.env 2>/dev/null)
if [ -z "$dbUser" ]
then
    echo 'Enter a username for the Postgres database.'
    read -p 'Username: ' dbUser
    export dbUser
else
    echo "Your database username: $dbUser"
fi

export password=$(grep -Po 'POSTGRES_PASSWORD=\K.*$' ./db/postgres/docker/.env 2>/dev/null)
if [ -z "$password" ]
then
    # Generate a random password that satisfies password requirements.
    echo 'A password is randomly being generated.'
    password=$(date +%s | sha256sum | base64 | head -c 29)A8!
    echo "Your generated password is: $password"
    export password
else
    echo "Your password is: $password"
fi

######################################################################
# Elasticsearch configuration
######################################################################

export elasticUser=$(grep -Po 'ELASTIC_USERNAME=\K.*$' ./db/elasticsearch/.env 2>/dev/null)
if [ -z "$elasticUser" ]
then
    echo 'Enter a username for Elasticsearch.'
    read -p 'Username: ' elasticUser
    export elasticUser
else
    echo "Your Elasticsearch username: $elasticUser"
fi

######################################################################
# Asure configuration
######################################################################

export azureCognitiveServiceKey=$(grep -Po 'COGNITIVE_SERVICES_SPEECH_SUBSCRIPTION_KEY=\K.*$' ./api/editor/.env 2>/dev/null)
if [ -z "$azureCognitiveServiceKey" ]
then
    echo 'Enter your Azure Cognitive Service subscription key.'
    read -p 'Key: ' azureCognitiveServiceKey
    export azureCognitiveServiceKey
else
    echo "Your Azure Cognitive Service subscription key: $azureCognitiveServiceKey"
fi

export azureCognitiveServiceRegion=$(grep -Po 'COGNITIVE_SERVICES_SPEECH_REGION=\K.*$' ./api/editor/.env 2>/dev/null)
if [ -z "$azureCognitiveServiceRegion" ]
then
    echo 'Enter your Azure Cognitive Service region (i.e. canadacentral).'
    read -p 'Region: ' azureCognitiveServiceRegion
    export azureCognitiveServiceRegion
else
    echo "Your Azure Cognitive Service region: $azureCognitiveServiceRegion"
fi

export azureVideoAnalyzerKey=$(grep -Po 'AZURE_VIDEO_ANALYZER_SUBSCRIPTION_KEY=\K.*$' ./api/editor/.env 2>/dev/null)
if [ -z "$azureVideoAnalyzerKey" ]
then
    echo 'Enter your Azure Video Analyzer subscription key.'
    read -p 'Key: ' azureVideoAnalyzerKey
    export azureVideoAnalyzerKey
else
    echo "Your Azure Video Analyzer subscription key: $azureVideoAnalyzerKey"
fi

export azureVideoAccountId=$(grep -Po 'AZURE_VIDEO_ANALYZER_ACCOUNT_ID=\K.*$' ./api/editor/.env 2>/dev/null)
if [ -z "$azureVideoAccountId" ]
then
    echo 'Enter your Azure Video Analyzer account ID.'
    read -p 'Account ID: ' azureVideoAccountId
    export azureVideoAccountId
else
    echo "Your Azure Video Analyzer account ID: $azureVideoAccountId"
fi

export azureVideoLocation=$(grep -Po 'AZURE_VIDEO_ANALYZER_LOCATION=\K.*$' ./api/editor/.env 2>/dev/null)
if [ -z "$azureVideoLocation" ]
then
    echo 'Enter your Azure Video Analyzer location (i.e. trial).'
    read -p 'Location: ' azureVideoLocation
    export azureVideoLocation
else
    echo "Your Azure Video Analyzer location: $azureVideoLocation"
fi

export azureKey=$(date +%s | sha256sum | base64 | head -c 29)

# Only required if the Azurite docker container doesn't allow for local domain names.
# Workaround is to either use 'mcr.microsoft.com/azure-storage/azurite:3.14.0', or use the IP address.
# echo 'Enter the IP of your local host.docker.internal.'
# read -p 'IP: ' varHostDockerInternal

######################################################################
# Port Configuration
######################################################################

export portNginx=40080
export portNginxHttps=40443

export portDatabase=40000
export portKeycloak=40001
export portKeycloakHttps=40002
export portElastic=40003
export portElasticCom=40004
export portDejavu=40005
export portAzureBlob=40006
export portAzureQueue=40007
export portAzureTable=40008

export portApiEditor=40010
export portApiEditorHttps=40011
export portAppEditor=40081
export portAppEditorHttps=40444
export portAppSubscriber=40082
export portAppSubscriberHttps=40445

export portSyndicationRss=40020
export portSyndicationAtom=40021
export portNlp=40022
export portIndexing=40023

export portKafkaZookeeper=40100
export portKafkaBrokerAdvertisedHost=40101
export portKafkaBorkerAdvertisedExternal=40102
export portKafkaSchemaRegistry=40103
export portKafkaRestProxy=40104
export portKafkaConnect=40105
export portKafkaKsqlDb=40106
export portKafkaKowl=40180
