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

message=$(echo $message | sed "s/$environmentPlaceholder/$TARGET_ENVIRONMENT/g")
message=$(echo $message | sed "s/$versionPlaceholder/$VERSION_NAME/g")
message+=" $buildUrl"

echo $message

echo "notificationMessage=$message" >> $GITHUB_OUTPUT