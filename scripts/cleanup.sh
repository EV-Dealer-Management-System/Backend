#!/bin/bash
CONTAINER_NAME=simple-docker-service

if [ $(docker ps -a -q -f name=$CONTAINER_NAME) ]; then
  echo "Removing old container"
  docker rm -f $CONTAINER_NAME
fi
