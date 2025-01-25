#!/bin/bash

# Check if a command-line argument was provided
if [ $# -eq 0 ]; then
    echo "Please provide the secret as an argument. You can find it in keycloak admin => mmi realm => clients => mmi-service-account. "
    exit 1
fi

# Use the first command-line argument as the account secret
account_secret="$1"

# Function to check if directory is TNO root by verifying key files/directories
is_tno_root() {
    local dir="$1"
    # Check for TNO.sln and key directories that are unique to TNO
    if [ -f "$dir/TNO.sln" ] && [ -d "$dir/api" ] && [ -d "$dir/services" ]; then
        return 0 # true
    fi
    return 1 # false
}

# Get the script's directory
script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" &> /dev/null && pwd)"

# Find TNO root by traversing up from script location
tno_root="$script_dir"
while [ "$tno_root" != "/" ]; do
    if is_tno_root "$tno_root"; then
        break
    fi
    tno_root="$(dirname "$tno_root")"
done

if [ "$tno_root" = "/" ]; then
    echo "Unable to locate the TNO root directory."
    exit 1
fi

# Files and their respective keys to check
declare -A files_keys=(
    ["$tno_root/api/net/.env"]="Keycloak__ServiceAccount__Secret="
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
