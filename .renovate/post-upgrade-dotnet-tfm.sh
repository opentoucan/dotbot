#!/bin/sh
NEW_SDK_VERSION=$(grep '"version":' ./global.json | cut -d '"' -f 4 | grep -Po '^[0-9]+')
sed -r -i -e 's|<TargetFramework>(net[0-9\.]+)</TargetFramework>|<TargetFramework>net'"${NEW_SDK_VERSION}"'.0</TargetFramework>|g' ./Directory.Packages.props
