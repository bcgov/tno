#!/bin/bash

# Ensure you are logged in.
if ! yarn npm whoami >/dev/null 2>&1; then
  echo "Logging in..."
  yarn npm login
fi

package_name=$(node -p "require('./package.json').name")
package_version=$(node -p "require('./package.json').version")

info=$()
current_version=$(yarn npm info tno-core version)

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

yarn config set registry https://registry.npmjs.org
yarn npm publish --access public

package_version=$(node -p "require('./package.json').version")
echo "version: $package_version"
