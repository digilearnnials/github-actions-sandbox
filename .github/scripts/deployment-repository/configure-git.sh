#!/bin/bash

git config --global user.name "$DEPLOYMENT_REPO_COMMIT_AUTHOR"
git config --global user.email "$DEPLOYMENT_REPO_COMMIT_EMAIL"