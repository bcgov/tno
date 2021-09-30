#!/bin/bash

echo 'Enter a username for the keycloak realm administrator'
read -p 'Username: ' varKeycloak

echo 'Enter a username for the API database.'
read -p 'Username: ' varApiDb

passvar=$(grep -Po 'POSTGRES_PASSWORD=\K.*$' ./db/postgres/.env)

if [ -z "$passvar" ]
then
  # Generate a random password that satisfies password requirements.
  echo 'A password is randomly being generated.'
  passvar=$(date +%s | sha256sum | base64 | head -c 29)A8!
  echo "Your generated password is: $passvar"
else
  echo "Your current password is: $passvar"
fi

# Set environment variables.
# Keycloak
if test -f "./auth/keycloak/.env"; then
    echo "./auth/keycloak/.env exists"
else
echo \
"PROXY_ADDRESS_FORWARDING=true
KEYCLOAK_USER=$varKeycloak
KEYCLOAK_PASSWORD=$passvar
KEYCLOAK_IMPORT=/tmp/realm-export.json -Dkeycloak.profile.feature.scripts=enabled -Dkeycloak.profile.feature.upload_scripts=enabled
KEYCLOAK_LOGLEVEL=WARN
ROOT_LOGLEVEL=WARN" >> ./auth/keycloak/.env
fi

# API Database
if test -f "./db/postgres/.env"; then
    echo "./db/postgres/.env exists"
else
echo \
"POSTGRES_USER=$varApiDb
POSTGRES_PASSWORD=$passvar
POSTGRES_DB=tno" >> ./db/postgres/.env
fi

# API - Editor
if test -f "./api/editor/api/src/main/resources/.env"; then
    echo "./api/editor/api/src/main/resources/.env exists"
else
echo \
"KEYCLOAK_AUTH_SERVER_URL=http://host.docker.internal:50000/auth/" >> ./api/editor/api/src/main/resources/.env
fi

# APP - Editor
if test -f "./app/editor/.env"; then
    echo "./app/editor/.env exists"
else
echo \
"NODE_ENV=development
CHOKIDAR_USEPOLLING=true
#API_URL=http://api-editor:8080/
REACT_APP_KEYCLOAK_AUTH_SERVER_URL=http://localhost:50000/auth" >> ./app/editor/.env
fi
