#!/bin/bash
ACCOUNT_ID=886350084162
REGION=ap-southeast-1
IMAGE_NAME=simple-docker-service
CONTAINER_NAME=simple-docker-service

echo "Starting new container"
docker run -d --name $CONTAINER_NAME -p 80:80 $ACCOUNT_ID.dkr.ecr.$REGION.amazonaws.com/$IMAGE_NAME:latest
