#!/bin/bash

echo ""
echo "*************************************"
echo "Setting up Signing Keys"
echo "*************************************"

echo ""
echo "Signing keys are required to deploy packages"
echo ""

if [ ! -f libs/java/dal/db/private.key ]; then
  gpg --gen-key

  echo 'Enter the key uid (normally your email address).'
  read -p 'key uid: ' varKeyUid

  gpg --export -a "$varKeyUid" > "libs/java/dal/db/public.key"
  gpg --export-secret-keys "$varKeyUid" > "libs/java/dal/db/private.key"
else
  echo 'You already have keys created.'
  gpg --list-keys
fi

export GPG_TTY=$(tty)
