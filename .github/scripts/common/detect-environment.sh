#!/bin/bash

environment=""

currentVersionParts=($(echo "$VERSION_NAME" | tr '-' '\n'))

versionLastPart=${currentVersionParts[1]}

if [ "$versionLastPart" = "dev" ]
then
    environment="development"
elif [ "$versionLastPart" = "test" ]
then
    environment="staging"
elif [ "$versionLastPart" = "prod" ]
then
    environment="production"
fi

echo "$environment"

echo "targetEnvironment=$environment" >> "$GITHUB_OUTPUT"