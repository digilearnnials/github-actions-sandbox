#!/bin/bash

message=$NOTIFICATION_MESSAGE

environmentPlaceholder="<environment>"
versionPlaceholder="<version>"

buildUrl="none"

case $TARGET_ENVIRONMENT in
    development)
    buildUrl=$DEVELOPMENT_BUILD_URL
    ;;
    staging)
    buildUrl=$STAGING_BUILD_URL
    ;;
    production)
    buildUrl=$PRODUCTION_BUILD_URL
    ;;
esac

message=${message//$environmentPlaceholder/$TARGET_ENVIRONMENT}
message=${message//$versionPlaceholder/$VERSION_NAME}

message+=" $buildUrl"

echo "$message"

echo "notificationMessage=$message" >> "$GITHUB_OUTPUT"