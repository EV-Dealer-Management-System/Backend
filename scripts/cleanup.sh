#!/bin/bash
CONTAINER_NAME=simple-docker-service

chmod +x scripts/*.sh

CONTAINER_ID=$(docker ps -a -q -f name=$CONTAINER_NAME)
if [ -n "$CONTAINER_ID" ]; then
  echo "Removing old container"
  docker rm -f $CONTAINER_ID
fi
