#!/bin/bash

cd deployment || exit

checkoutBranch="none"

case $TARGET_ENVIRONMENT in
    Development)
    checkoutBranch=$DEPLOYMENT_REPO_DEVELOPMENT_BRANCH
    ;;
    Staging)
    checkoutBranch=$DEPLOYMENT_REPO_STAGING_BRANCH
    ;;
    Production)
    checkoutBranch=$DEPLOYMENT_REPO_PRODUCTION_BRANCH
    ;;
esac

git checkout "$checkoutBranch"

latestCommitId="$(git rev-parse HEAD)"

if [ "$latestCommitId" != "$DEPLOYMENT_REPO_HARD_RESET_COMMIT_ID" ]
then
    git reset --hard "$DEPLOYMENT_REPO_HARD_RESET_COMMIT_ID"
    git push --force
fi