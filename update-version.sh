#!/bin/bash

set -e

VERSION=$1

if [[ -z "$VERSION" ]]; then
  echo "Version not provided"
  exit 1
fi

echo "Updating .csproj files to version $VERSION"

for file in $(find . -name '*.csproj'); do
  sed -i "s|<AssemblyVersion>.*</AssemblyVersion>|<AssemblyVersion>$VERSION</AssemblyVersion>|g" "$file"
  sed -i "s|<FileVersion>.*</FileVersion>|<FileVersion>$VERSION</FileVersion>|g" "$file"
  sed -i "s|<Version>.*</Version>|<Version>$VERSION</Version>|g" "$file"
done

echo "Version update complete"
