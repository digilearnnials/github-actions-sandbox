environment="development"

git fetch --tags

versions=($(git tag --list --sort -version:refname --column))

if (( ${#versions[@]} > 1 ))
then
    currentVersion=$GITHUB_REF_NAME
    previousVersion=${versions[0]}
    currentNums=()
    previousNums=()
    
    readarray -d . -t currentNums <<< $currentVersion
    readarray -d . -t previousNums <<< $previousVersion
    
    if (( ${currentNums[0]} > ${previousNums[0]} ))
    then
        environment="production"
    elif (( ${currentNums[1]} > ${previousNums[1]} ))
    then
        environment="staging"
    fi
fi

echo "targetEnvironment=$environment" >> $GITHUB_OUTPUT