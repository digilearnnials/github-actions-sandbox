#!/bin/bash

environment=""

currentVersionParts=($(echo "$VERSION_NAME" | tr '-' '\n'))

versionLastPart=${currentVersionParts[1]}

if [ "$versionLastPart" = "dev" ]
then
    environment="Development"
elif [ "$versionLastPart" = "test" ]
then
    environment="Staging"
elif [ "$versionLastPart" = "prod" ]
then
    environment="Production"
fi

echo "$environment"

echo "targetEnvironment=$environment" >> "$GITHUB_OUTPUT"