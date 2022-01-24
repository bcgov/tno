#!/bin/bash

# Import the private.key that is in the root folder.
# This is required to sign and verify packages.
# sudo env "PATH=$PATH" gpg --import private.key]
FILE=private.key
if [ -f "$FILE" ]; then
  if [ "$1" = "sudo" ]; then
    sudo gpg --import "$FILE"
    sudo cp .devcontainer/gpg.conf ~/.gnupg/gpg.conf
    sudo cp .devcontainer/gpg-agent.conf ~/.gnupg/gpg-agent.conf
  else
    gpg --import "$FILE"
    cp .devcontainer/gpg.conf ~/.gnupg/gpg.conf
    cp .devcontainer/gpg-agent.conf ~/.gnupg/gpg-agent.conf
  fi
fi

echo RELOADAGENT | gpg-connect-agent

# sudo echo 'default-key:0:"xxxxxx' | gpgconf —change-options gpg