#!/bin/bash

buildSucceeded=false

if [ -d "build/$BUILD_TARGET" ]
then
    echo "Success!"
    buildSucceeded=true;
else
    echo "Failure!"
    buildSucceeded=false;
fi

echo "buildSucceeded=$buildSucceeded" >> "$GITHUB_OUTPUT"