#!/bin/bash

releaseFolder="release"
zipFolder=""

mkdir $releaseFolder

if [ "$BUILD_TARGET" = "WebGL" ]
then
    cp -R "build/$BUILD_TARGET/$BUILD_FULL_NAME" $releaseFolder
    zipFolder=$BUILD_FULL_NAME
else
    cp -R "build/$BUILD_TARGET" $releaseFolder
    zipFolder=$BUILD_TARGET
fi

cd release || exit

oldStr=" "
newStr="_"
zipName=${BUILD_FULL_NAME//$oldStr/$newStr}

zip -r "$zipName.zip" "$zipFolder"

echo "targetZipPath=$releaseFolder/$zipName.zip" >> "$GITHUB_OUTPUT"