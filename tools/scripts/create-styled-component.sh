#!/bin/sh
# Create a styled component with the given name, this script will also create the styled folder if none exists already as well as the
# index.ts file if none exists already. If they do exist, it will append the export statement to the index.ts file.
# Usage: create-styled-component.sh <component-name>
# Example: create-styled-component.sh Button
# Create a symbolic link to this file in your /usr/local/bin directory to use it from anywhere.
# Example: ln -s ~/create-styled-component.sh /usr/local/bin/create-styled-component
if [ $# -eq 0 ]; then
	echo "No arguments supplied, please provide a component name."
	exit 1
fi

if [ -f $1.tsx ]; then
	echo "Component with name $1 already exists."
	exit 1
fi

echo "Creating styled component in $(pwd)..."

# -p flag creates the directory if it doesn't exist already
mkdir -p styled

if [ ! -f styled/index.ts ]; then
  echo "Creating styled/index.ts..."
  touch styled/index.ts
fi

touch styled/$1.tsx
echo "import styled from 'styled-components';" >> styled/$1.tsx
touch $1.tsx

if [ -f index.ts ]; then
	echo "Appending export statement to index.ts..."
	echo "export * from './$1';" >> index.ts
fi

echo "import React from 'react';" >> $1.tsx
echo "import * as styled from './styled';" >> $1.tsx
echo "export * from './$1';" >> styled/index.ts

echo "Done..."
