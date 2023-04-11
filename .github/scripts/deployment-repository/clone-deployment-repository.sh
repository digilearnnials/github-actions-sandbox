repositoryUrl=$DEPLOYMENT_REPO_URL

usernamePlaceholder="<username>"
appPasswordPlaceholder="<app-password>"

repositoryUrl=$(echo $repositoryUrl | sed "s/$usernamePlaceholder/$DEPLOYMENT_REPO_USERNAME/g")
repositoryUrl=$(echo $repositoryUrl | sed "s/$appPasswordPlaceholder/$DEPLOYMENT_REPO_APP_PASSWORD/g")

git clone $repositoryUrl deployment