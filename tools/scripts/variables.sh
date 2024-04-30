#!/bin/bash

. ./tools/scripts/os.sh

######################################################################
# Keycloak configuration
######################################################################

export keycloakDbName="keycloak"

export keycloakUser=$("$GREP" -Po 'KEYCLOAK_USER=\K.*$' ./auth/keycloak/.env 2>/dev/null)
if [ -z "$keycloakUser" ]
then
    echo 'Enter a username for the keycloak realm administrator'
    read -p 'Username: ' keycloakUser
    export keycloakUser
else
    echo "Your keycloak username: $keycloakUser"
fi

export keycloakPassword=$("$GREP" -Po 'KEYCLOAK_PASSWORD=\K.*$' ./auth/keycloak/.env 2>/dev/null)
if [ -z "$keycloakPassword" ]
then
    echo 'Enter a password for the keycloak realm administrator'
    read -p 'Password: ' keycloakPassword
    export keycloakPassword
else
    echo "Your keycloak password: $keycloakPassword"
fi

######################################################################
# Postgres Database configuration
######################################################################

export dbName="tno"

export dbUser=$("$GREP" -Po 'POSTGRES_USER=\K.*$' ./db/postgres/docker/.env 2>/dev/null)
if [ -z "$dbUser" ]
then
    echo 'Enter a username for the Postgres database.'
    read -p 'Username: ' dbUser
    export dbUser
else
    echo "Your database username: $dbUser"
fi

export password=$("$GREP" -Po 'POSTGRES_PASSWORD=\K.*$' ./db/postgres/docker/.env 2>/dev/null)
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
# Oracle Database configuration
######################################################################

export oraclePassword=$("$GREP" -Po 'ORACLE_PWD=\K.*$' ./db/oracle/docker/.env 2>/dev/null)
if [ -z "$oraclePassword" ]
then
    # Generate a random password that satisfies password requirements.
    echo 'A password is randomly being generated for connection to the oracle db.'
    oraclePassword=$(date +%s | sha256sum | base64 | head -c 29)A8!
    echo "Your generated password is: $oraclePassword"
    export oraclePassword
else
    echo "Your password is: $oraclePassword"
fi

######################################################################
# Elasticsearch configuration
######################################################################

export elasticUser=elastic

######################################################################
# Asure configuration
######################################################################

export azureCognitiveServiceKey=$("$GREP" -Po 'COGNITIVE_SERVICES_SPEECH_SUBSCRIPTION_KEY=\K.*$' ./api/net/.env 2>/dev/null)
if [ -z "$azureCognitiveServiceKey" ]
then
    echo 'Enter your Azure Cognitive Service subscription key.'
    read -p 'Key: ' azureCognitiveServiceKey
    export azureCognitiveServiceKey
else
    echo "Your Azure Cognitive Service subscription key: $azureCognitiveServiceKey"
fi

export azureCognitiveServiceRegion=$("$GREP" -Po 'COGNITIVE_SERVICES_SPEECH_REGION=\K.*$' ./api/net/.env 2>/dev/null)
if [ -z "$azureCognitiveServiceRegion" ]
then
    echo 'Enter your Azure Cognitive Service region (i.e. canadacentral).'
    read -p 'Region: ' azureCognitiveServiceRegion
    export azureCognitiveServiceRegion
else
    echo "Your Azure Cognitive Service region: $azureCognitiveServiceRegion"
fi

export azureVideoAnalyzerKey=$("$GREP" -Po 'AZURE_VIDEO_ANALYZER_SUBSCRIPTION_KEY=\K.*$' ./api/net/.env 2>/dev/null)
if [ -z "$azureVideoAnalyzerKey" ]
then
    echo 'Enter your Azure Video Analyzer subscription key.'
    read -p 'Key: ' azureVideoAnalyzerKey
    export azureVideoAnalyzerKey
else
    echo "Your Azure Video Analyzer subscription key: $azureVideoAnalyzerKey"
fi

export azureVideoAccountId=$("$GREP" -Po 'AZURE_VIDEO_ANALYZER_ACCOUNT_ID=\K.*$' ./api/net/.env 2>/dev/null)
if [ -z "$azureVideoAccountId" ]
then
    echo 'Enter your Azure Video Analyzer account ID.'
    read -p 'Account ID: ' azureVideoAccountId
    export azureVideoAccountId
else
    echo "Your Azure Video Analyzer account ID: $azureVideoAccountId"
fi

export azureVideoLocation=$("$GREP" -Po 'AZURE_VIDEO_ANALYZER_LOCATION=\K.*$' ./api/net/.env 2>/dev/null)
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

export portNginxEditor=40080
export portNginxSubscriber=40081
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
export portOracleDatabase=41521

export portApi=40010
export portApiHttps=40011
export portCssApi=40012
export portCssApiHttps=40013
export portChartsApi=40014
export portChartsApiHttps=40015
export portAppEditor=40082
export portAppEditorHttps=40444
export portAppSubscriber=40083
export portAppSubscriberHttps=40445

export portSyndication=40020
export portCapture=40021
export portClip=40022
export portImage=40023
export portFile=40024
export portContent=40025
export portIndexing=40026
export portTranscription=40027
export portNlp=40028
export portCoreNlp=40038
export portFileCopy=40029
export portNotification=40030
export portReporting=40031
export portContentMigration=40032
export portFolderCollection=40033
export portFFmpeg=40034
export portScheduler=40035
export portEventHandler=40036
export portExtractQuotes=40037

export portKafkaBrokerAdvertisedHost=40101
export portKafkaBrokerAdvertisedExternal=40102
export portKafkaKowl=40180
