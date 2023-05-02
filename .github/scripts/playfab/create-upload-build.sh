#!/bin/bash

function getJsonData ()
{
	requestData="" 
	
	requestFile="$1"

	while read -r line; do
		line=${line//"\n"/" "}
		requestData+=$line
	done < "$requestFile"

	requestData+="}"

	echo "$requestData"
}

function getFieldValueFromJson ()
{
	fieldValue=""

	json="$1"
	fieldName="$2"

	fieldValue=$(echo "$json" | sed -E -n 's/.*'"$fieldName"'":"([^"]+).+/\1/p')

	echo "$fieldValue"
}

function getBuildAliasId ()
{
	buidAliasId="" 

	case $TARGET_ENVIRONMENT in
		Development)
		buidAliasId=$PLAYFAB_ALIAS_ID_DEVELOPMENT
		;;
		Staging)
		buidAliasId=$PLAYFAB_ALIAS_ID_STAGING
		;;
		Production)
		buidAliasId=$PLAYFAB_ALIAS_ID_PRODUCTION
		;;
	esac

	echo "$buidAliasId"
}


basePath=$(cd "$(dirname "${BASH_SOURCE[0]}")" || exit ; pwd -P)

tokenResponse=$(curl -s -X POST "$PLAYFAB_BASE_URL/Authentication/GetEntityToken" \
				     -H "Content-Type: application/json" \
				     -H "X-SecretKey: $PLAYFAB_SECRET_KEY")

entityToken=$(getFieldValueFromJson "$tokenResponse" "EntityToken")

createBuildRequestFile="$basePath/requests-data/create-build-with-custom-container.json"
createBuildRequest=$(getJsonData "$createBuildRequestFile")

buildName="$SERVER_BUILD_NAME""_""$VERSION_NAME"

buildNamePlaceholder="<build-name>"
imageNamePlaceholder="<image-name>"
tagPlaceholder="<tag>"

createBuildRequest=${createBuildRequest//$buildNamePlaceholder/$buildName}
createBuildRequest=${createBuildRequest//$imageNamePlaceholder/$IMAGE_NAME}
createBuildRequest=${createBuildRequest//$tagPlaceholder/$VERSION_NAME}

createBuildResponse=$(curl -s -X POST "$PLAYFAB_BASE_URL/MultiplayerServer/CreateBuildWithCustomContainer" \
					       -H "Content-Type: application/json" \
					       -H "X-EntityToken: $entityToken" \
					       -d "$createBuildRequest")

echo "CREATE BUILD RESPONSE: $createBuildResponse"

aliasId=$(getBuildAliasId)
buildId=$(getFieldValueFromJson "$createBuildResponse" "BuildId")

updateBuildAliasRequestFile="$basePath/requests-data/update-build-alias-request.json"
updateBuildAliasRequest=$(getJsonData "$updateBuildAliasRequestFile")

aliasIdPlaceholder="<alias-id>"
aliasNamePlaceholder="<alias-name>"
buildIdPlaceholder="<build-id>"

updateBuildAliasRequest=${updateBuildAliasRequest//$aliasIdPlaceholder/$aliasId}
updateBuildAliasRequest=${updateBuildAliasRequest//$aliasNamePlaceholder/$TARGET_ENVIRONMENT}
updateBuildAliasRequest=${updateBuildAliasRequest//$buildIdPlaceholder/$buildId}

updateBuildAliasResponse=$(curl -s -X POST "$PLAYFAB_BASE_URL/MultiplayerServer/UpdateBuildAlias" \
						   -H "Content-Type: application/json" \
						   -H "X-EntityToken: $entityToken" \
						   -d "$updateBuildAliasRequest")

echo "UPDATE BUILD ALIAS RESPONSE: $updateBuildAliasResponse"