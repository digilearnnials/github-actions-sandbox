#!/bin/bash

mkdir release

if [ "$BUILD_TARGET" = "WebGL" ]
then
    cp -R "build/$BUILD_TARGET/$BUILD_FULL_NAME" release
else
    cp -R "build/$BUILD_TARGET" release
fi

cd release || exit

oldStr=" "
newStr="_"
zipName=${BUILD_FULL_NAME//$oldStr/$newStr}

zip -r "$zipName.zip" "$BUILD_FULL_NAME"

echo "targetZipName=$zipName" >> "$GITHUB_OUTPUT"