#!/bin/bash

if [[ "$OSTYPE" == "linux-gnu"* ]]; then # Linux
    GREP=$(which grep)
elif [[ "$OSTYPE" == "darwin"* ]]; then # Macos
    GREP=$(which ggrep)
elif [[ "$OSTYPE" == "cygwin" ]]; then # Linux emulator for Windows
    GREP=$(which grep)
elif [[ "$OSTYPE" == "msys" ]]; then # GNU for Windows
    GREP=$(which grep)
elif [[ "$OSTYPE" == "win32" ]]; then # Windows
    GREP=$(which grep)
elif [[ "$OSTYPE" == "freebsd"* ]]; then
    GREP=$(which grep)
else
    GREP=$(which grep)
fi
