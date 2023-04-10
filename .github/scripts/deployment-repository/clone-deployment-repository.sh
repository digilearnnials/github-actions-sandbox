repositoryUrl=$DEPLOYMENT_REPO_URL

usernamePlaceholder="<username>"
actualUsername=$DEPLOYMENT_REPO_USERNAME

repositoryUrl=$(echo $repositoryUrl | sed "s/$usernamePlaceholder/$actualUsername/g")

appPasswordPlaceholder="<app-password>"
actualAppPassword=$DEPLOYMENT_REPO_APP_PASSWORD

repositoryUrl=$(echo $repositoryUrl | sed "s/$appPasswordPlaceholder/$actualAppPassword/g")

echo $repositoryUrl

git clone $repositoryUrl deployment