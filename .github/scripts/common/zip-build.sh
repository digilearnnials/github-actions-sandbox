#!/bin/bash

zipFolder=""

mkdir release

if [ "$BUILD_TARGET" = "WebGL" ]
then
    cp -R "build/$BUILD_TARGET/$BUILD_FULL_NAME" release
    zipFolder=$BUILD_FULL_NAME
else
    cp -R "build/$BUILD_TARGET" release
    zipFolder=$BUILD_TARGET
fi

cd release || exit

oldStr=" "
newStr="_"
zipName=${BUILD_FULL_NAME//$oldStr/$newStr}

zip -r "$zipName.zip" "$zipFolder"

echo "targetZipName=$zipName" >> "$GITHUB_OUTPUT"