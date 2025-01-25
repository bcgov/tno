#!/bin/bash

# Function to display usage
show_usage() {
    echo "Usage: $0 [-s secret] [-id clientId]"
    echo "Options:"
    echo "  -s     Set the secret value for Keycloak__ServiceAccount__Secret and Auth__Keycloak__Secret"
    echo "  -id    Set the Keycloak__ClientId value"
    echo "You can find the secret in keycloak admin => mmi realm => clients => mmi-service-account."
    exit 1
}

# Parse command line arguments
secret=""
client_id=""

while [ "$#" -gt 0 ]; do
    case "$1" in
        -s)
            secret="$2"
            shift 2
            ;;
        -id)
            client_id="$2"
            shift 2
            ;;
        *)
            echo "Unknown parameter: $1"
            show_usage
            ;;
    esac
done

# Debug output
echo "Debug: secret=$secret, client_id=$client_id"

# Check if at least one argument was provided
if [ -z "$secret" ] && [ -z "$client_id" ]; then
    echo "Error: At least one option (-s or -id) must be provided."
    show_usage
fi

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

# Function to update value in file
update_value() {
    local file="$1"
    local key="$2"
    local new_value="$3"

    if [ -f "$file" ]; then
        # Get the old value, handling both regular values and placeholder values
        if grep -q "^[^#]*$key" "$file"; then
            local line=$(grep "^[^#]*$key" "$file")
            local old_value="${line#*$key}"
            # Use a different delimiter for sed to avoid conflicts
            sed -i "s|^[^#]*$key.*|$key$new_value|" "$file"
            echo "Modified: $file"
            echo "$key$old_value => $key$new_value"
            echo
        fi
    else
        echo "File not found: $file"
        echo
    fi
}

# Update secrets if provided
if [ -n "$secret" ]; then
    echo "Updating secrets..."
    # Update Keycloak__ServiceAccount__Secret in api/net/.env
    update_value "$tno_root/api/net/.env" "Keycloak__ServiceAccount__Secret=" "$secret"

    # Update Auth__Keycloak__Secret in all service .env files
    for dir in $tno_root/services/net/*/ ; do
        env_file="${dir}.env"
        update_value "$env_file" "Auth__Keycloak__Secret=" "$secret"
    done
fi

# Update client ID if provided
if [ -n "$client_id" ]; then
    echo "Updating client ID..."
    update_value "$tno_root/api/net/.env" "Keycloak__ClientId=" "$client_id"
fi
