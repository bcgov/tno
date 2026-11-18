#!/bin/bash
echo "start script..."
# install tools
apk add --no-cache curl jq

# download mc
curl https://dl.min.io/client/mc/release/linux-amd64/mc -o /usr/local/bin/mc
chmod +x /usr/local/bin/mc

# config MinIO client
mc alias set mys3 $S3_ENDPOINT $S3_ACCESS_KEY $S3_SECRET_KEY

# define dirs
SOURCE_DIR="/av"
DEST_DIR="backups"

# mc mirror 
mc mirror --overwrite --preserve $SOURCE_DIR mys3/$S3_BUCKET/$DEST_DIR

# print 
echo "finish syncing"
