#!/bin/bash

read_env() {
    [ -f "$2" ] || return 0
    awk -v key="$1" 'index($0, key "=") == 1 {sub(/^[^=]*=/, ""); sub(/\r$/, ""); print; exit}' "$2"
}
random_value() {
    LC_ALL=C od -An -N24 -tx1 /dev/urandom | tr -d ' \n' | cut -c1-29
}

######################################################################
# Keycloak configuration
######################################################################

export keycloakDbName="keycloak"

export keycloakUser=$(read_env KEYCLOAK_ADMIN ./dependencies/keycloak/.env)
if [ -z "$keycloakUser" ]
then
    echo 'Enter a username for the keycloak realm administrator'
    read -p 'Username: ' keycloakUser
    export keycloakUser
else
    echo "Your keycloak username: $keycloakUser"
fi

export keycloakPassword=$(read_env KEYCLOAK_ADMIN_PASSWORD ./dependencies/keycloak/.env)
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

export dbUser=$(read_env POSTGRES_USER ./dependencies/postgres/docker/.env)
if [ -z "$dbUser" ]
then
    echo 'Enter a username for the Postgres database.'
    read -p 'Username: ' dbUser
    export dbUser
else
    echo "Your database username: $dbUser"
fi

export password=$(read_env POSTGRES_PASSWORD ./dependencies/postgres/docker/.env)
if [ -z "$password" ]
then
    # Generate a random password that satisfies password requirements.
    echo 'A password is randomly being generated.'
    password=$(random_value)A8!
    echo "Your generated password is: $password"
    export password
else
    echo "Your password is: $password"
fi

######################################################################
# Oracle Database configuration
######################################################################

export oraclePassword=$(read_env ORACLE_PWD ./dependencies/oracle/docker/.env)
if [ -z "$oraclePassword" ]
then
    # Generate a random password that satisfies password requirements.
    echo 'A password is randomly being generated for connection to the oracle db.'
    oraclePassword=$(random_value)A8!
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

export azureCognitiveServiceKey=$(read_env COGNITIVE_SERVICES_SPEECH_SUBSCRIPTION_KEY ./src/api/.env)
if [ -z "$azureCognitiveServiceKey" ]
then
    echo 'Enter your Azure Cognitive Service subscription key.'
    read -p 'Key: ' azureCognitiveServiceKey
    export azureCognitiveServiceKey
else
    echo "Your Azure Cognitive Service subscription key: $azureCognitiveServiceKey"
fi

export azureCognitiveServiceRegion=$(read_env COGNITIVE_SERVICES_SPEECH_REGION ./src/api/.env)
if [ -z "$azureCognitiveServiceRegion" ]
then
    echo 'Enter your Azure Cognitive Service region (i.e. canadacentral).'
    read -p 'Region: ' azureCognitiveServiceRegion
    export azureCognitiveServiceRegion
else
    echo "Your Azure Cognitive Service region: $azureCognitiveServiceRegion"
fi

export azureVideoAnalyzerKey=$(read_env AZURE_VIDEO_ANALYZER_SUBSCRIPTION_KEY ./src/api/.env)
if [ -z "$azureVideoAnalyzerKey" ]
then
    echo 'Enter your Azure Video Analyzer subscription key.'
    read -p 'Key: ' azureVideoAnalyzerKey
    export azureVideoAnalyzerKey
else
    echo "Your Azure Video Analyzer subscription key: $azureVideoAnalyzerKey"
fi

export azureVideoAccountId=$(read_env AZURE_VIDEO_ANALYZER_ACCOUNT_ID ./src/api/.env)
if [ -z "$azureVideoAccountId" ]
then
    echo 'Enter your Azure Video Analyzer account ID.'
    read -p 'Account ID: ' azureVideoAccountId
    export azureVideoAccountId
else
    echo "Your Azure Video Analyzer account ID: $azureVideoAccountId"
fi

export azureVideoLocation=$(read_env AZURE_VIDEO_ANALYZER_LOCATION ./src/api/.env)
if [ -z "$azureVideoLocation" ]
then
    echo 'Enter your Azure Video Analyzer location (i.e. trial).'
    read -p 'Location: ' azureVideoLocation
    export azureVideoLocation
else
    echo "Your Azure Video Analyzer location: $azureVideoLocation"
fi

export azureKey=$(random_value)

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
export portAzureBlob=40006
export portAzureQueue=40007
export portAzureTable=40008
export portOracleDatabase=41521

export portApi=40010
export portApiHttps=40011
export portChartsApi=40014
export portChartsApiHttps=40015
export portAppEditor=40082
export portAppEditorHttps=40444
export portAppSubscriber=40083
export portAppSubscriberHttps=40445

export portSyndication=40020
export portImage=40023
export portFile=40024
export portContent=40025
export portIndexing=40026
export portTranscription=40027
export portAutoClipper=40029
export portNotification=40030
export portReporting=40031
export portContentMigration=40032
export portFolderCollection=40033
export portFFmpeg=40034
export portScheduler=40035
export portEventHandler=40036

export portKafkaBrokerAdvertisedHost=40101
export portKafkaBrokerAdvertisedExternal=40102
export portKafkaUI=40180
