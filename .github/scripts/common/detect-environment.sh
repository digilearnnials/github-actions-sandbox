#!/bin/bash

environment=""

git fetch --tags

currentVersion=$VERSION_NAME
currentVersionParts=$("$currentVersion" | tr "-")

if [ ${#currentVersionParts[@]} == 1 ]
then
    environment="production"
elif [ "${currentVersionParts[1]}" == "test" ]
then
    environment="staging"
else
    environment="development"
fi

echo "targetEnvironment=$environment" >> "$GITHUB_OUTPUT"