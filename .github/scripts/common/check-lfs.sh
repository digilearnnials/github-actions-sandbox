#!/bin/bash

isUsingLFS=false;

if git grep -q filter=lfs -- .gitattributes '**/.gitattributes'
then 
    echo "The project is using LFS."
    isUsingLFS=true;
else 
    echo "The project is not using LFS."
    isUsingLFS=false;
fi

echo "isUsingLFS=$isUsingLFS" >> "$GITHUB_OUTPUT"
