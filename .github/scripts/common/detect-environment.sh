#!/bin/bash

targetEnvironment=""

currentVersionParts=($(echo "$VERSION_NAME" | tr '-' '\n'))

versionLastPart=${currentVersionParts[1]}

case $versionLastPart in
    dev)
    targetEnvironment="Development"
    ;;
    test)
    targetEnvironment="Staging"
    ;;
    prod)
    targetEnvironment="Production"
    ;;
    *)
    targetEnvironment="Development"
esac

echo $targetEnvironment >> "$GITHUB_OUTPUT"