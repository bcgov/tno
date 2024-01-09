#!/bin/bash
if [[ "$1" ]]; then
  yarn run build
  package_name=$(node -p "require('./package.json').name")
  package_version=$(node -p "require('./package.json').version")
  package=${package_name}-${package_version}
  latest_version=$(npm show $package_name version)

  echo "Pack ${package}"

  # Create a tgz package file.
  cd dist
  yarn pack --out ../%s-%v.tgz

  # Place the local tgz package in the editor app location.
  echo "Move package ${package} to project"

  # Handle both in devcontainer or out.
  if [ -f "/workspaces/tno/app/editor/" ]; then
    mv -f ../${package}.tgz /workspaces/tno/app/$1/
    cd /workspaces/tno/app/$1
  else
    mv -f ../${package}.tgz ../../../../app/$1/
    cd ../../../../app/$1
  fi

  # Need to do this or it'll ignore additional changes to the same version.
  echo "Clear cache"
  yarn cache clean --all

  # Need to do this or it'll fail to install the local tgz package.
  echo "Install ${package_name}@${latest_version}"
  rm -f .yarn/cache/${package_name}-*.zip
  yarn add ${package_name}@${latest_version}
  yarn install

  # Finally install the local tgz package to test.
  echo "Install local ${package_name}@${package_version}"
  yarn add ./${package}.tgz
  yarn install

  echo "Run 'make refresh n=$1' in the root directory to rebuild the frontend app."
else
  echo "*Please provide either n=subscriber or n=editor*"
fi
