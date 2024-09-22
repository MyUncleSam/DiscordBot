#!/bin/bash
set -e
echo "Starting build workflow"

. scripts/git_release_version.sh
scripts/docker_initialize.sh

if [ -z "${VERSION}" ]; then
    echo "Version not set or not found"
    exit 1
fi

cd source/DiscordBot
echo "Building docker image from remote Dockerfile for version ${VERSION}"

# run build
echo "[${BRANCH_NAME}] Building images: ${IMAGE_FULLNAME}"
if [ "$BRANCH_NAME" = "master" ] || [ "$BRANCH_NAME" = "main" ]
then
    docker build \
        -t ${IMAGE_FULLNAME}:${VERSION} \
        -t ${IMAGE_FULLNAME}:latest \
        -f Dockerfile \
        --push .
else
    docker build \
        -t ${IMAGE_FULLNAME}-test:${BRANCH_NAME}-${VERSION} \
        -f Dockerfile \
        --push .
fi

# cleanup
cd -
scripts/docker_cleanup.sh
