#!/bin/bash

repositoryUrl=$DEPLOYMENT_REPO_URL

usernamePlaceholder="<username>"
appPasswordPlaceholder="<app-password>"

repositoryUrl=${repositoryUrl//$usernamePlaceholder/$DEPLOYMENT_REPO_USERNAME}
repositoryUrl=${repositoryUrl//$appPasswordPlaceholder/$DEPLOYMENT_REPO_APP_PASSWORD}

git clone "$repositoryUrl" deployment