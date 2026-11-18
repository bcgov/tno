#!/bin/bash

echo ""
echo "*************************************"
echo "Setting up Signing Keys"
echo "*************************************"

echo ""
echo "Signing keys are required to deploy packages"
echo ""

echo 'Enter the "key-id" (normally your email address) that you use to generate signed keys'
read -p 'key id: ' varKeyUid

# If key doesn't exist, then create it
if [ ! "$(gpg -k $varKeyUid | grep -w $varKeyUid)" ]; then
  gpg --gen-key
fi

##################################################
# Export key to each project
##################################################

# Core
if [ ! -f src/libs/java/core/private.key ]; then
  gpg --export -a "$varKeyUid" > "src/libs/java/core/public.key"
  gpg --export-secret-keys "$varKeyUid" > "src/libs/java/core/private.key"
fi

# DAL DB
if [ ! -f src/libs/java/dal/db/private.key ]; then
  gpg --export -a "$varKeyUid" > "src/libs/java/dal/db/public.key"
  gpg --export-secret-keys "$varKeyUid" > "src/libs/java/dal/db/private.key"
fi

# DAL Elastic
if [ ! -f src/libs/java/dal/elastic/private.key ]; then
  gpg --export -a "$varKeyUid" > "src/libs/java/dal/elastic/public.key"
  gpg --export-secret-keys "$varKeyUid" > "src/libs/java/dal/elastic/private.key"
fi

# Service
if [ ! -f src/libs/java/service/private.key ]; then
  gpg --export -a "$varKeyUid" > "src/libs/java/service/public.key"
  gpg --export-secret-keys "$varKeyUid" > "src/libs/java/service/private.key"
fi

gpg --list-keys

export GPG_TTY=$(tty)
