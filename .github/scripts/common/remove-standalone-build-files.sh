#!/bin/bash

cd "build/$BUILD_TARGET" || exit

rm -rf "$BUILD_FULL_NAME""_""$DO_NOT_SHIP_FOLDER_SUFFIX"
rm "$CRASH_HANDLER_FILE_NAME.exe"