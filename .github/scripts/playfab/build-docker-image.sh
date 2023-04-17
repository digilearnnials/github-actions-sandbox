#!/bin/bash

cp -R "build/$BUILD_TARGET/"* Docker/bin

cd "Docker" || exit

docker login -u "$DOCKER_USERNAME" -p "$DOCKER_PASSWORD" "$DOCKER_SERVER_URL"
docker build -t "$DOCKER_SERVER_URL/$DOCKER_IMAGE_NAME:$TAG_NAME" .
docker push "$DOCKER_SERVER_URL/$DOCKER_IMAGE_NAME:$TAG_NAME"