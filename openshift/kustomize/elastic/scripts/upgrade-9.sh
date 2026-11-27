#!/bin/bash
# Prepare an Elasticsearch 8.19 cluster for 9.x. See ../UPGRADE-9.5.7.md.
#
#   upgrade-9.sh [--local] <command> [arguments]
#
# Targets the OpenShift cluster by default (namespace $NS, default 9b301c-dev; `oc login` first).
# --local targets the docker-compose container ($LOCAL_CONTAINER, default tno-elastic) and reads
# ELASTIC_PASSWORD from dependencies/elasticsearch/.env when it is not already set.
#
# A 9.x node refuses to start while any index created on 7.x is neither reindexed nor read-only.
#
#   check                     List what blocks 9.x: critical deprecations, system features to migrate,
#                             and every index created before 8.0 with its aliases.
#   rebuild <index>           Recreate an index created on 7.x under the same name, keeping its mappings,
#                             settings, aliases and document versions. Stop every writer to it first.
#                             Reads keep working: the aliases point at a temporary copy meanwhile.
#   delete-index <index>      Delete an index that no alias points at.
#   delete-data-stream <name> Delete a data stream and its backing indexes.
#   system-features           Migrate the system indexes (.security and others) and wait until done.
set -euo pipefail

NS="${NS:-9b301c-dev}"
POD="${POD:-elastic-0}"
LOCAL_CONTAINER="${LOCAL_CONTAINER:-tno-elastic}"

LOCAL=false
if [ "${1:-}" = "--local" ]; then
  LOCAL=true
  shift
fi

if $LOCAL && [ -z "${ELASTIC_PASSWORD+x}" ]; then
  ENV_FILE="$(git rev-parse --show-toplevel 2>/dev/null || pwd)/dependencies/elasticsearch/.env"
  if [ -f "$ENV_FILE" ]; then
    ELASTIC_PASSWORD="$(grep -E '^ELASTIC_PASSWORD=' "$ENV_FILE" | cut -d= -f2- | tr -d "'\"")"
  fi
fi

# es <METHOD> <path> [json body] - call Elasticsearch from inside the container/pod.
# Prints the response body and fails (exit 1) when Elasticsearch answers with HTTP 400 or above.
es() {
  local method="$1" path="$2" body="${3:-}" output status
  local args=(curl -sS -m 3600 -w '\n%{http_code}' -X "$method" -H 'Content-Type: application/json' "http://localhost:9200$path")
  [ -n "$body" ] && args+=(-d "$body")
  if $LOCAL; then
    if [ -n "${ELASTIC_PASSWORD:-}" ]; then
      # Pass the credentials through the environment so they never appear in a process list.
      output="$(docker exec -e "ES_AUTH=elastic:$ELASTIC_PASSWORD" "$LOCAL_CONTAINER" sh -c 'exec "$@" -u "$ES_AUTH"' sh "${args[@]}")"
    else
      output="$(docker exec "$LOCAL_CONTAINER" "${args[@]}")"
    fi
  else
    output="$(oc exec -n "$NS" "$POD" -c elastic -- "${args[@]}")"
  fi
  status="${output##*$'\n'}"
  printf '%s\n' "${output%$'\n'*}"
  if [ "$status" -ge 400 ] 2>/dev/null; then
    echo "ERROR: $method $path returned HTTP $status" >&2
    return 1
  fi
}

require() {
  if [ -z "${1:-}" ]; then
    echo "ERROR: $2" >&2
    exit 1
  fi
}

count() {
  es POST "/$1/_refresh" >/dev/null
  es GET "/$1/_count" | python3 -c 'import json,sys; print(json.load(sys.stdin)["count"])'
}

# copy <source> <destination> - create the destination from the source's mappings and settings, then copy
# every document with its version (the indexing service writes with external versions).
copy() {
  local source="$1" destination="$2" from to
  es POST "/_create_from/$source/$destination" >/dev/null
  es POST "/_reindex?wait_for_completion=true&refresh=true" \
    "{\"source\":{\"index\":\"$source\"},\"dest\":{\"index\":\"$destination\",\"version_type\":\"external\"}}" >/dev/null
  from="$(count "$source")"
  to="$(count "$destination")"
  if [ "$from" != "$to" ]; then
    echo "ERROR: '$source' has $from document(s) but '$destination' has $to; stopping." >&2
    exit 1
  fi
  echo "Copied $to document(s): $source -> $destination"
}

# aliases <index> - the index's aliases as {"alias": {properties}}.
aliases() {
  es GET "/$1/_alias?expand_wildcards=all" | python3 -c 'import json,sys; print(json.dumps(next(iter(json.load(sys.stdin).values()), {}).get("aliases", {})))'
}

# move_aliases <aliases json> <from> <to> - move the aliases atomically, keeping their properties.
move_aliases() {
  local actions
  actions="$(python3 -c '
import json,sys
aliases, source, target = json.loads(sys.argv[1]), sys.argv[2], sys.argv[3]
actions = []
for name, props in aliases.items():
    actions.append({"remove": {"index": source, "alias": name}})
    actions.append({"add": dict(props, index=target, alias=name)})
print(json.dumps({"actions": actions}))' "$1" "$2" "$3")"
  [ "$actions" = '{"actions": []}' ] || es POST /_aliases "$actions" >/dev/null
}

case "${1:-}" in
  check)
    echo "== Critical deprecations (each must be resolved before 9.x starts)"
    es GET /_migration/deprecations | python3 -c '
import json,sys
d = json.load(sys.stdin)
found = False
for kind, value in d.items():
    items = [(name, x) for name, l in value.items() for x in l] if isinstance(value, dict) else [("", x) for x in value]
    for name, x in items:
        if x["level"] == "critical":
            found = True
            message = x["message"]
            print(f"  {kind:16} {name:60} {message}")
print("  none" if not found else "", end="")'
    echo
    echo "== System features"
    es GET '/_migration/system_features?filter_path=migration_status'
    echo "== Indexes created before 8.0, with their aliases"
    es GET '/_all/_settings/index.version.created?expand_wildcards=all&flat_settings=true' | python3 -c '
import json,sys
for name, v in sorted(json.load(sys.stdin).items()):
    created = v["settings"]["index.version.created"]
    if int(created) < 8000099:
        print(f"  {name}")' | while read -r index; do
      echo "  $index  aliases: $(aliases "$index" | tr -d '{}')"
    done
    ;;
  rebuild)
    index="${2:-}"
    require "$index" "usage: upgrade-9.sh [--local] rebuild <index>"
    temporary="$index-rebuild-9"
    if es GET "/$temporary/_settings" >/dev/null 2>&1; then
      echo "ERROR: '$temporary' already exists (a previous run stopped part way). Check it, delete it, then rerun." >&2
      exit 1
    fi
    current="$(aliases "$index")"
    echo "== $index (aliases: $current)"
    es PUT "/$index/_block/write" >/dev/null
    copy "$index" "$temporary"
    move_aliases "$current" "$index" "$temporary"
    es DELETE "/$index" >/dev/null
    copy "$temporary" "$index"
    move_aliases "$current" "$temporary" "$index"
    es DELETE "/$temporary" >/dev/null
    echo "Rebuilt '$index' (created $(es GET "/$index/_settings/index.version.created?flat_settings=true" | python3 -c 'import json,sys; print(next(iter(json.load(sys.stdin).values()))["settings"]["index.version.created"])'))."
    ;;
  delete-index)
    index="${2:-}"
    require "$index" "usage: upgrade-9.sh [--local] delete-index <index>"
    if [ "$(aliases "$index")" != "{}" ]; then
      echo "ERROR: '$index' has aliases $(aliases "$index"); rebuild it instead." >&2
      exit 1
    fi
    es DELETE "/$index" >/dev/null
    echo "Deleted '$index'."
    ;;
  delete-data-stream)
    name="${2:-}"
    require "$name" "usage: upgrade-9.sh [--local] delete-data-stream <name>"
    es DELETE "/_data_stream/$name?expand_wildcards=all" >/dev/null
    echo "Deleted data stream '$name'."
    ;;
  system-features)
    es POST /_migration/system_features
    until es GET '/_migration/system_features?filter_path=migration_status' | grep -q NO_MIGRATION_NEEDED; do
      es GET '/_migration/system_features?filter_path=migration_status'
      sleep 10
    done
    echo "System features migrated."
    ;;
  *)
    sed -n '2,20p' "$0" | sed 's/^# \{0,1\}//'
    exit 1
    ;;
esac
