#!/bin/bash
ACCOUNT_ID=886350084162
REGION=ap-southeast-1
IMAGE_NAME=simple-docker-service

echo "Login to ECR"
aws ecr get-login-password --region $REGION | docker login --username AWS --password-stdin $ACCOUNT_ID.dkr.ecr.$REGION.amazonaws.com

echo "Pulling latest image"
docker pull $ACCOUNT_ID.dkr.ecr.$REGION.amazonaws.com/$IMAGE_NAME:latest
