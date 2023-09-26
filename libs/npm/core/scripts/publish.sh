#!/bin/bash

user=$(yarn npm whoami)

if [[ "$user" == *"No authentication configured for request"* ]]; then
  yarn npm login
fi

package_name=$(node -p "require('./package.json').name")
package_version=$(node -p "require('./package.json').version")

info=$()
current_version=$(yarn npm info tno-core | grep -oP '(?<="customfield_11500": ")[^"]*' -)

echo "Current version $package_version"

echo "Enter the version number you will publish"
echo "m: major"
echo "i: minor"
echo "p: patch"
read -p 'Option, or Version: ' version

if [ "$version" == "m" ]; then
  version="major"
elif [ "$version" == "i" ]; then
  version="minor"
elif [ "$version" == "p" ]; then
  version="patch"
fi
npm version $version

yarn npm publish

package_version=$(node -p "require('./package.json').version")
echo "version: $package_version"
