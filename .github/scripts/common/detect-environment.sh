#!/bin/bash

environment=""

currentVersion=$VERSION_NAME
currentVersionParts=()

readarray -d - -t currentVersionParts <<< "$currentVersion"

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