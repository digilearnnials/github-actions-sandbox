function getJsonData ()
{
	requestData="" 
	
	requestFile="$1"

	while read line; do
		line=$(echo $line | sed "s/\n/ /g")
		requestData+=$line
	done < "$requestFile"

	requestData+="}"

	echo $requestData
}

function getFieldValueFromJson ()
{
	fieldValue=""

	json="$1"
	fieldName="$2"

	fieldValue=$(echo $json | sed -E -n 's/.*'$fieldName'":"([^"]+).+/\1/p')

	echo $fieldValue
}

function getBuildAliasId ()
{
	buidAliasId="" 

	case $TARGET_ENVIRONMENT in
		development)
		buidAliasId=$PLAYFAB_ALIAS_ID_DEVELOPMENT
		;;
		staging)
		buidAliasId=$PLAYFAB_ALIAS_ID_STAGING
		;;
		production)
		buidAliasId=$PLAYFAB_ALIAS_ID_PRODUCTION
		;;
	esac

	echo $buidAliasId
}

basePath=$(cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P)

tokenResponse=$(curl -s -X POST "$PLAYFAB_BASE_URL/Authentication/GetEntityToken" \
				     -H "Content-Type: application/json" \
				     -H "X-SecretKey: $PLAYFAB_SECRET_KEY")

entityToken=$(getFieldValueFromJson $tokenResponse "EntityToken")

createBuildRequestFile="$basePath/requests-data/create-build-with-custom-container.json"
createBuildRequest=$(getJsonData "$createBuildRequestFile")

buildName=$SERVER_BUILD_NAME"_"$TARGET_ENVIRONMENT"_"$VERSION_NAME

buildNamePlaceholder="<build-name>"
tagPlaceholder="<tag>"

createBuildRequest=$(echo $createBuildRequest | sed "s/$buildNamePlaceholder/$buildName/g")
createBuildRequest=$(echo $createBuildRequest | sed "s/$tagPlaceholder/$VERSION_NAME/g")

createBuildResponse=$(curl -s -X POST "$PLAYFAB_BASE_URL/MultiplayerServer/CreateBuildWithCustomContainer" \
					       -H "Content-Type: application/json" \
					       -H "X-EntityToken: $entityToken" \
					       -d "$createBuildRequest")

aliasId=$(getBuildAliasId)
buildId=$(getFieldValueFromJson $createBuildResponse "BuildId")

updateBuildAliasRequestFile="$basePath/requests-data/update-build-alias-request.json"
updateBuildAliasRequest=$(getJsonData "$updateBuildAliasRequestFile")

aliasIdPlaceholder="<alias-id>"
aliasNamePlaceholder="<alias-name>"
buildIdPlaceholder="<build-id>"

updateBuildAliasRequest=$(echo $updateBuildAliasRequest | sed "s/$aliasIdPlaceholder/$aliasId/g")
updateBuildAliasRequest=$(echo $updateBuildAliasRequest | sed "s/$aliasNamePlaceholder/$TARGET_ENVIRONMENT/g")
updateBuildAliasRequest=$(echo $updateBuildAliasRequest | sed "s/$buildIdPlaceholder/$buildId/g")

echo $updateBuildAliasRequest

curl -s -X POST "$PLAYFAB_BASE_URL/MultiplayerServer/UpdateBuildAlias" \
	 -H "Content-Type: application/json" \
	 -H "X-EntityToken: $entityToken" \
	 -d "$updateBuildAliasRequest"