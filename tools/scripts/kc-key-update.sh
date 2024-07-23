#!/bin/bash

# Check if a command-line argument was provided
if [ $# -eq 0 ]; then
    echo "Please provide the secret as an argument. You can find it in keycloak admin => mmi realm => clients => mmi-service-account. "
    exit 1
fi

# Use the first command-line argument as the account secret
account_secret="$1"

# Dynamically obtain the absolute path of the script
script_path="$(realpath "$0")"

# Find the tno directory in the path
regex="(.*/mmi)/"
if [[ $script_path =~ $regex ]]; then
    tno_root="${BASH_REMATCH[1]}"
else
    echo "Unable to locate the tno directory in the script path."
    exit 1
fi

# Files and their respective keys to check
declare -A files_keys=(
    ["$tno_root/tools/css-api/.env"]="Keycloak__Secret="
    ["$tno_root/api/net/.env"]="CSS__Secret="
)

# Check and modify specified files
for file in "${!files_keys[@]}"; do
    key=${files_keys[$file]}
    if [ -f "$file" ]; then
        # Check and ignore commented lines
        if grep -q "^[^#]*$key" "$file"; then
            old_value=$(grep "^[^#]*$key" "$file" | sed -n "s/.*$key\(.*\)/\1/p")
            # Use a different delimiter, e.g., `#`, to avoid potential conflicts
            sed -i "/^[^#]*$key/c$key$account_secret" "$file"
            echo "Modified: $file"
            echo "$key$old_value => $key$account_secret"
            echo
        fi
    else
        echo "File not found: $file"
        echo
    fi
done

# Loop through all directories under tno/services/net/ and check .env files
for dir in $tno_root/services/net/*/ ; do
    env_file="${dir}.env"
    if [ -f "$env_file" ]; then
        key="Auth__Keycloak__Secret="
        if grep -q "^[^#]*$key" "$env_file"; then
            old_value=$(grep "^[^#]*$key" "$env_file" | sed -n "s/.*$key\(.*\)/\1/p")
            # Use a different delimiter, e.g., `#`, to avoid potential conflicts
            sed -i "/^[^#]*$key/c$key$account_secret" "$env_file"
            echo "Modified: $env_file"
            echo "$key$old_value => $key$account_secret"
            echo
        fi
    else
        echo "Directory not found or .env file does not exist in: $dir"
        echo
    fi
done
