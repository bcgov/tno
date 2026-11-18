#!/bin/sh
# Starts nginx for the design named by APP_MODE. There is no default: a deployment without a valid
# APP_MODE fails to start, so a misconfigured rollout never replaces the running pods.
set -eu
case "${APP_MODE:-}" in
  editor | subscriber) ;;
  *)
    echo "FATAL: APP_MODE must be 'editor' or 'subscriber' (got '${APP_MODE:-<unset>}')." >&2
    exit 64
    ;;
esac
exec nginx -c "/etc/nginx/mmi/nginx.${APP_MODE}.conf" -g 'daemon off;'
