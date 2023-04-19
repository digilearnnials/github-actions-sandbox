#!/bin/bash

mkdir release

cp -R "build/$BUILD_TARGET/$BUILD_FULL_NAME" release

cd release || exit

oldStr=" "
newStr="_"
zipName=${BUILD_FULL_NAME//$oldStr/$newStr}

zip -r "$zipName.zip" "$BUILD_FULL_NAME"

echo "targetZipName=$zipName" >> "$GITHUB_OUTPUT"