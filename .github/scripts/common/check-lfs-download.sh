#!/bin/bash

if git grep -q filter=lfs -- .gitattributes '**/.gitattributes'
then 
    echo "The project is using LFS. Pulling LFS content..."
    git lfs pull
else 
    echo "The project is not using LFS. Skipping LFS pull..."
fi
