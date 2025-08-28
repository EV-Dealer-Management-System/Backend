#!/bin/bash
CONTAINER_NAME=simple-docker-service

if [ $(docker ps -q -f name=$CONTAINER_NAME) ]; then
  echo "Stopping old container"
  docker stop $CONTAINER_NAME
fi
