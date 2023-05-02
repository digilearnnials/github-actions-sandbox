#!/bin/bash

message=$NOTIFICATION_MESSAGE

environmentPlaceholder="<environment>"
versionPlaceholder="<version>"

buildUrl="none"

case $TARGET_ENVIRONMENT in
    Development)
    buildUrl=$DEVELOPMENT_BUILD_URL
    ;;
    Staging)
    buildUrl=$STAGING_BUILD_URL
    ;;
    Production)
    buildUrl=$PRODUCTION_BUILD_URL
    ;;
esac

message=${message//$environmentPlaceholder/$TARGET_ENVIRONMENT}
message=${message//$versionPlaceholder/$VERSION_NAME}

message+=" $buildUrl"

echo "$message"

echo "notificationMessage=$message" >> "$GITHUB_OUTPUT"