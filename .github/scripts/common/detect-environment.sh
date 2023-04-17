#!/bin/bash

environment=""

currentVersionParts=()

readarray -d - -t currentVersionParts <<< "$VERSION_NAME"

if [ "${currentVersionParts[1]}" == "dev" ]
then
    environment="development"
elif [ "${currentVersionParts[1]}" == "test" ]
then
    environment="staging"
elif [ "${currentVersionParts[1]}" == "prod" ]
then
    environment="production"
fi

echo "targetEnvironment=$environment" >> "$GITHUB_OUTPUT"