#!/bin/bash
CONTAINER_NAME=simple-docker-service

if [ $(docker ps -q -f name=$CONTAINER_NAME) ]; then
  docker stop $CONTAINER_NAME
fi
