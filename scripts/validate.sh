#!/bin/bash
echo "Validating service"
curl -f http://localhost:80 || exit 1
