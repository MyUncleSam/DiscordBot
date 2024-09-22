#!/bin/bash
set -e
echo "Get the latest build release"

URL="https://api.github.com/repos/davidje13/Refacto/releases/latest"
GITHUB_RELEASE=$(curl -s "$URL")
export VERSION=$(echo ${GITHUB_RELEASE} | jq '.name' )
