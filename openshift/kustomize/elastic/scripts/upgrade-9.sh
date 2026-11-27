#!/bin/bash
# Prepare an Elasticsearch 8.19 cluster for 9.x. See ../UPGRADE-9.5.7.md.
#
#   upgrade-9.sh [--local] <command> [argument]
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
#                             settings, aliases and document versions. STOP EVERY WRITER FIRST: the index and
#                             its temporary copy (<index>-rebuild-9) refuse writes until the rebuild is done,
#                             so a writer that is still running fails instead of losing documents. Reads keep
#                             working: the aliases point at the temporary copy meanwhile.
#                             Rerun the same command after a failure. It works out how far the last run got
#                             and resumes; it never deletes the only complete copy.
#   unblock <index>           Remove the write block `rebuild` put on an index. Only for abandoning a rebuild
#                             that stopped before the original was deleted.
#   delete-index <index>      Delete an index that no alias points at.
#   delete-data-stream <name> Delete a data stream and its backing indexes.
#   system-features           Migrate the system indexes (.security and others) and wait until done.
#
# Every command stops with an error when Elasticsearch does not answer, or answers with anything but HTTP 2xx.
#
# Environment:
#   ES_TIMEOUT                Seconds to wait for one Elasticsearch call, including a whole copy (default 3600).
#   SYSTEM_FEATURES_TIMEOUT   Seconds `system-features` waits for the migration to finish (default 1800).
set -euo pipefail

NS="${NS:-9b301c-dev}"
POD="${POD:-elastic-0}"
LOCAL_CONTAINER="${LOCAL_CONTAINER:-tno-elastic}"
ES_TIMEOUT="${ES_TIMEOUT:-3600}"
SYSTEM_FEATURES_TIMEOUT="${SYSTEM_FEATURES_TIMEOUT:-1800}"
# The temporary copy `rebuild` works through is named <index>$SUFFIX.
SUFFIX="-rebuild-9"
# Lowercase names only: no list (,), wildcard (*), path (/), query (?), fragment (#), quote (") or _all.
INDEX_NAME='^[a-z0-9][a-z0-9._-]*$'
# Elasticsearch's own data streams start with a dot (.logs-deprecation.elasticsearch-default).
DATA_STREAM_NAME='^[.]?[a-z0-9][a-z0-9._-]*$'

SELF="$0"
LOCAL=false
if [ "${1:-}" = "--local" ]; then
  LOCAL=true
  SELF="$0 --local"
  shift
fi

usage() {
  awk 'NR > 1 && /^#/ { sub(/^# ?/, ""); print; next } NR > 1 { exit }' "$0"
  exit 1
}

# env_value <file> <NAME> - the value of NAME=value in an env file. One pair of matching quotes around the
# whole value is removed, as docker-compose does; any other quote character is part of the value.
env_value() {
  local value
  value="$(sed -n "s/^$2=//p" "$1" | tail -n 1)"
  value="${value%$'\r'}"
  if [ "${#value}" -ge 2 ]; then
    case "$value" in
      \"*\" | \'*\') value="${value:1:${#value}-2}" ;;
    esac
  fi
  printf '%s' "$value"
}

if $LOCAL && [ -z "${ELASTIC_PASSWORD+x}" ]; then
  ENV_FILE="$(git rev-parse --show-toplevel 2>/dev/null || pwd)/dependencies/elasticsearch/.env"
  if [ -f "$ENV_FILE" ]; then
    ELASTIC_PASSWORD="$(env_value "$ENV_FILE" ELASTIC_PASSWORD)"
  fi
fi

# curl_quote <text> - quote a value for a curl config file (curl -K).
curl_quote() {
  local text="$1"
  text="${text//\\/\\\\}"
  text="${text//\"/\\\"}"
  text="${text//$'\t'/\\t}"
  text="${text//$'\n'/\\n}"
  text="${text//$'\r'/\\r}"
  printf '"%s"' "$text"
}

# es [--allow-404] <METHOD> <path> [json body] - call Elasticsearch from inside the container/pod.
#
# Prints the response body and returns 0 only for HTTP 2xx. Anything else prints the reason and the response to
# standard error and returns 1: another status, or no response at all (the container or pod is not running, the
# connection is refused, the call times out). With --allow-404, HTTP 404 returns 4 silently so the caller can
# tell "does not exist" apart from a failure.
#
# The credentials and the request body reach curl as a config file on standard input (curl -K -). The password is
# therefore in no argument list and no environment, on this host or in the container.
es() {
  local allow_404=false
  if [ "$1" = "--allow-404" ]; then
    allow_404=true
    shift
  fi
  local method="$1" path="$2" body="${3:-}" config="" output status="" code=0
  local run=(oc exec -i -n "$NS" "$POD" -c elastic --)
  if $LOCAL; then
    run=(docker exec -i "$LOCAL_CONTAINER")
    if [ -n "${ELASTIC_PASSWORD:-}" ]; then
      config+="user = $(curl_quote "elastic:$ELASTIC_PASSWORD")"$'\n'
    fi
  fi
  if [ -n "$body" ]; then
    config+="data-binary = $(curl_quote "$body")"$'\n'
  fi
  output="$(printf '%s' "$config" | "${run[@]}" curl -sS -K - --connect-timeout 10 -m "$ES_TIMEOUT" \
    -w '\n%{http_code}' -X "$method" -H 'Content-Type: application/json' "http://localhost:9200$path")" || code=$?
  # curl writes the status after the body. It is 000, or missing, when no response arrived.
  if [[ "$output" == *$'\n'* ]]; then
    status="${output##*$'\n'}"
    output="${output%$'\n'*}"
  fi
  if [ "$code" -ne 0 ] || ! [[ "$status" =~ ^[1-5][0-9][0-9]$ ]]; then
    echo "ERROR: $method $path got no complete response from Elasticsearch (exit $code, HTTP status '${status:-none}')." >&2
    return 1
  fi
  if [[ "$status" =~ ^2[0-9][0-9]$ ]]; then
    printf '%s\n' "$output"
    return 0
  fi
  if $allow_404 && [ "$status" = "404" ]; then
    return 4
  fi
  echo "ERROR: $method $path returned HTTP $status" >&2
  [ -z "$output" ] || printf '%s\n' "$output" >&2
  return 1
}

fail() {
  echo "ERROR: $1" >&2
  exit 1
}

# require_name <value> <pattern> <what> - stop unless the argument is one plain name.
require_name() {
  if ! [[ "$1" =~ $2 ]]; then
    fail "'$1' is not a valid $3 name. Give exactly one name (lowercase letters, digits, '.', '_' and '-'); lists and wildcards are refused."
  fi
}

# index_aliases <index> - print the aliases of the index with exactly this name, as {"alias": {properties}}.
# Returns 4 when Elasticsearch answers 404 and 1 on any other failure. A name that resolves to something else
# (an alias or a data stream) is a failure, never an empty result.
index_aliases() {
  local name="$1" response code=0
  response="$(es --allow-404 GET "/$name/_alias?expand_wildcards=all")" || code=$?
  [ "$code" -eq 0 ] || return "$code"
  python3 -c '
import json, sys
name, found = sys.argv[1], json.load(sys.stdin)
if list(found) != [name] or not isinstance(found[name].get("aliases"), dict):
    sys.exit(f"ERROR: \"{name}\" is not an index: the lookup returned {sorted(found)}.")
print(json.dumps(found[name]["aliases"]))' "$name" <<<"$response"
}

# exists <index> - 0 when the index exists, 1 when Elasticsearch answers 404. Stops the script on anything else,
# so a failed lookup is never read as "exists" or as "missing". Call it from the script itself, not from $( ).
exists() {
  local code=0
  index_aliases "$1" >/dev/null || code=$?
  case "$code" in
    0) return 0 ;;
    4) return 1 ;;
    *) exit 1 ;;
  esac
}

# aliases <index> - index_aliases for an index that must exist.
aliases() {
  local code=0
  index_aliases "$1" || code=$?
  if [ "$code" -eq 4 ]; then
    echo "ERROR: index '$1' does not exist." >&2
  fi
  [ "$code" -eq 0 ]
}

# setting <index> <name> - print one setting of an index (e.g. index.blocks.write), or nothing when it is not set.
setting() {
  local response
  response="$(es GET "/$1/_settings/$2?flat_settings=true&expand_wildcards=all")" || return 1
  python3 -c '
import json, sys
print(json.load(sys.stdin).get(sys.argv[1], {}).get("settings", {}).get(sys.argv[2], ""))' "$1" "$2" <<<"$response"
}

# count <index> - refresh the index and print how many documents it holds. Fails when any shard did not answer.
count() {
  local response
  response="$(es POST "/$1/_refresh")" || return 1
  python3 -c '
import json, sys
if json.load(sys.stdin)["_shards"]["failed"]:
    sys.exit(f"ERROR: refreshing \"{sys.argv[1]}\" failed on some shards.")' "$1" <<<"$response" || return 1
  response="$(es GET "/$1/_count")" || return 1
  python3 -c '
import json, sys
found = json.load(sys.stdin)
if found["_shards"]["failed"]:
    sys.exit(f"ERROR: counting \"{sys.argv[1]}\" failed on some shards.")
print(found["count"])' "$1" <<<"$response"
}

# block_write <index> - make the index refuse writes, and confirm that it does.
block_write() {
  local blocked
  es PUT "/$1/_block/write" >/dev/null || exit 1
  blocked="$(setting "$1" index.blocks.write)" || exit 1
  [ "$blocked" = "true" ] || fail "'$1' has no write block after one was requested."
}

# stop_if_reindexing <index>... - a copy started by an interrupted run keeps running inside Elasticsearch.
# Deleting or recreating an index underneath it leaves a partial copy, so stop until that copy has finished.
stop_if_reindexing() {
  local response
  response="$(es GET '/_tasks?actions=indices:data/write/reindex&detailed=true')" || exit 1
  python3 -c '
import json, sys
found = []
for node in json.load(sys.stdin).get("nodes", {}).values():
    for task_id, task in node.get("tasks", {}).items():
        description = task.get("description") or ""
        if any(f"[{name}]" in description for name in sys.argv[1:]):
            found.append(f"  {task_id}  {description}")
if found:
    print("ERROR: a copy from an earlier run is still running inside Elasticsearch:", *found, sep="\n", file=sys.stderr)
    print("Wait until GET /_tasks/<id> answers 404 (or stop it with POST /_tasks/<id>/_cancel), then rerun.", file=sys.stderr)
    sys.exit(1)' "$@" <<<"$response" || exit 1
}

# copy <source> <destination> - create the destination from the source's mappings and settings (without its
# write block), then copy every document with its version (the indexing service writes with external versions).
copy() {
  local source="$1" destination="$2" response from to
  es POST "/_create_from/$source/$destination" >/dev/null || exit 1
  response="$(es POST "/_reindex?wait_for_completion=true&refresh=true" \
    "{\"source\":{\"index\":\"$source\"},\"dest\":{\"index\":\"$destination\",\"version_type\":\"external\"}}")" || exit 1
  python3 -c '
import json, sys
result = json.load(sys.stdin)
problems = []
failures, canceled = result.get("failures"), result.get("canceled")
if failures:
    problems.append(f"{len(failures)} failure(s), the first: {json.dumps(failures[0])}")
if result.get("timed_out"):
    problems.append("timed_out is set")
if canceled:
    problems.append(f"canceled: {canceled}")
if problems:
    sys.exit(f"ERROR: the copy {sys.argv[1]} -> {sys.argv[2]} did not complete: " + "; ".join(problems))' \
    "$source" "$destination" <<<"$response" || exit 1
  from="$(count "$source")" || exit 1
  to="$(count "$destination")" || exit 1
  if [ "$from" != "$to" ]; then
    fail "'$source' has $from document(s) but '$destination' has $to; stopping."
  fi
  echo "Copied $to document(s): $source -> $destination"
}

# move_aliases <from> <to> - move every alias of one index to another atomically, keeping their properties.
move_aliases() {
  local from="$1" to="$2" current actions
  current="$(aliases "$from")" || exit 1
  actions="$(python3 -c '
import json, sys
source, target = sys.argv[1], sys.argv[2]
actions = []
for name, properties in json.load(sys.stdin).items():
    actions.append({"remove": {"index": source, "alias": name}})
    actions.append({"add": dict(properties, index=target, alias=name)})
if actions:
    print(json.dumps({"actions": actions}))' "$from" "$to" <<<"$current")" || exit 1
  if [ -n "$actions" ]; then
    es POST /_aliases "$actions" >/dev/null || exit 1
    echo "Moved aliases $current: $from -> $to"
  fi
}

# rebuild_stopped - when a rebuild fails, say what state it left and what to do next.
rebuild_stopped() {
  local code=$?
  if [ "$code" -ne 0 ] && [ -n "${advice:-}" ]; then
    printf '\nThe rebuild stopped. %s\n' "$advice" >&2
  fi
}

case "${1:-}" in
  check)
    echo "== Critical deprecations (each must be resolved before 9.x starts)"
    response="$(es GET /_migration/deprecations)" || exit 1
    python3 -c '
import json, sys
found = False
for kind, value in json.load(sys.stdin).items():
    items = [(name, x) for name, l in value.items() for x in l] if isinstance(value, dict) else [("", x) for x in value]
    for name, x in items:
        if x["level"] == "critical":
            found = True
            message = x["message"]
            print(f"  {kind:16} {name:60} {message}")
if not found:
    print("  none")' <<<"$response"
    echo "== System features (ready when NO_MIGRATION_NEEDED)"
    response="$(es GET '/_migration/system_features?filter_path=migration_status')" || exit 1
    echo "  $response"
    echo "== Indexes created before 8.0, with their aliases"
    response="$(es GET '/_all/_settings/index.version.created?expand_wildcards=all&flat_settings=true')" || exit 1
    old="$(python3 -c '
import json, sys
for name, v in sorted(json.load(sys.stdin).items()):
    if int(v["settings"]["index.version.created"]) < 8000099:
        print(name)' <<<"$response")" || exit 1
    if [ -z "$old" ]; then
      echo "  none"
    else
      while IFS= read -r index; do
        current="$(aliases "$index")" || exit 1
        echo "  $index  aliases: $current"
      done <<<"$old"
    fi
    ;;
  rebuild)
    [ $# -eq 2 ] || usage
    index="$2"
    require_name "$index" "$INDEX_NAME" index
    if [[ "$index" == *"$SUFFIX" ]]; then
      fail "'$index' is the temporary copy of a rebuild. Name the original: $SELF rebuild ${index%"$SUFFIX"}"
    fi
    temporary="$index$SUFFIX"
    stop_if_reindexing "$index" "$temporary"

    # Where did the last run get to? The steps are:
    #   1 block writes to the original and copy it to the temporary index
    #   2 block writes to the temporary index and move the aliases onto it
    #   3 delete the original          (from here the temporary index is the only complete copy)
    #   4 recreate the original from the temporary index
    #   5 move the aliases back
    #   6 delete the temporary index
    has_original=false
    has_temporary=false
    if exists "$index"; then has_original=true; fi
    if exists "$temporary"; then has_temporary=true; fi

    abandon="  $SELF delete-index $temporary    # only if that index exists
  $SELF unblock $index"
    intact="'$index' is intact. Rerun the same command to try again.
To abandon the rebuild instead, so that '$index' accepts writes again:
$abandon"
    only_copy="'$temporary' holds the ONLY complete copy of '$index': do not delete it.
Fix the cause, then rerun the same command; it resumes from '$temporary'."
    unknown="Nothing was deleted. Check '$index' and '$temporary' before changing anything, and never delete one
unless the other is complete."
    advice=""
    trap rebuild_stopped EXIT

    if ! $has_original && ! $has_temporary; then
      fail "index '$index' does not exist."
    elif $has_original && ! $has_temporary; then
      step=1
    elif ! $has_original; then
      advice="$only_copy"
      blocked="$(setting "$temporary" index.blocks.write)" || exit 1
      if [ "$blocked" != "true" ]; then
        advice="$unknown"
        fail "'$temporary' exists without '$index', but it has no write block, so this script did not finish copying into it."
      fi
      echo "== Resuming: '$index' is gone and '$temporary' holds the complete copy."
      step=4
    else
      advice="$unknown"
      created_original="$(setting "$index" index.creation_date)" || exit 1
      created_temporary="$(setting "$temporary" index.creation_date)" || exit 1
      blocked_original="$(setting "$index" index.blocks.write)" || exit 1
      blocked_temporary="$(setting "$temporary" index.blocks.write)" || exit 1
      aliases_original="$(aliases "$index")" || exit 1
      aliases_temporary="$(aliases "$temporary")" || exit 1
      state="  $index: created ${created_original:-?}, write block ${blocked_original:-none}, aliases $aliases_original
  $temporary: created ${created_temporary:-?}, write block ${blocked_temporary:-none}, aliases $aliases_temporary"
      if ! [[ "$created_original" =~ ^[0-9]+$ && "$created_temporary" =~ ^[0-9]+$ ]]; then
        fail "cannot tell which of the two indexes is older:
$state"
      elif [ "$created_original" -lt "$created_temporary" ]; then
        # The original was never deleted, so it is complete.
        if [ "$aliases_temporary" = "{}" ]; then
          echo "== Starting again: '$temporary' is a partial or unused copy from an interrupted run, and '$index' is intact."
          es DELETE "/$temporary" >/dev/null || exit 1
          step=1
        elif [ "$blocked_original" = "true" ] && [ "$blocked_temporary" = "true" ]; then
          # Aliases only move after the copy was verified and write-blocked.
          echo "== Resuming: the aliases already point at the verified copy '$temporary'."
          step=2
        else
          fail "'$temporary' has aliases, but one of the two indexes has lost its write block, so they may differ:
$state"
        fi
      elif [ "$created_original" -gt "$created_temporary" ] && [ "$blocked_temporary" = "true" ]; then
        # The original was deleted and recreated from the temporary index, which is the complete copy.
        advice="$only_copy"
        from="$(count "$temporary")" || exit 1
        to="$(count "$index")" || exit 1
        if [ "$to" -eq "$from" ]; then
          echo "== Resuming: '$index' was recreated and holds all $to document(s)."
          step=5
        elif [ "$to" -lt "$from" ] && [ "$aliases_original" = "{}" ]; then
          echo "== Resuming: '$index' was recreated but holds $to of $from document(s); copying again from '$temporary'."
          es DELETE "/$index" >/dev/null || exit 1
          step=4
        else
          advice="$unknown"
          fail "'$index' was recreated from '$temporary' but no longer matches it ($to document(s) against $from), so it has been written to since. '$temporary' is the copy from before the rebuild. If '$index' is correct and holds the aliases, finish with '$SELF delete-index $temporary'.
$state"
        fi
      else
        fail "'$index' and '$temporary' both exist in a state this script did not leave:
$state"
      fi
    fi

    echo "== $index (rebuilding through $temporary)"
    if [ "$step" -le 1 ]; then
      advice="$intact"
      block_write "$index"
      echo "'$index' now refuses writes. If the rebuild is abandoned before it finishes, remove the write block with:
$abandon"
      copy "$index" "$temporary"
    fi
    if [ "$step" -le 2 ]; then
      advice="$intact"
      # _create_from dropped the write block. Put it back before the aliases move, so that a write through an
      # alias fails instead of landing in a copy that is deleted at the end.
      block_write "$temporary"
      es POST "/$temporary/_refresh" >/dev/null || exit 1
      move_aliases "$index" "$temporary"
    fi
    if [ "$step" -le 3 ]; then
      advice="$only_copy"
      es DELETE "/$index" >/dev/null || exit 1
    fi
    if [ "$step" -le 4 ]; then
      advice="$only_copy"
      copy "$temporary" "$index"
    fi
    if [ "$step" -le 5 ]; then
      advice="'$index' is rebuilt and complete. Rerun the same command to move the aliases back and remove '$temporary'."
      move_aliases "$temporary" "$index"
    fi
    es DELETE "/$temporary" >/dev/null || exit 1
    advice=""
    created="$(setting "$index" index.version.created)" || exit 1
    blocked="$(setting "$index" index.blocks.write)" || exit 1
    [ "$blocked" != "true" ] || fail "'$index' is rebuilt (created $created) but still has a write block. Remove it with '$SELF unblock $index'."
    echo "Rebuilt '$index' (created $created). It accepts writes again."
    ;;
  unblock)
    [ $# -eq 2 ] || usage
    index="$2"
    require_name "$index" "$INDEX_NAME" index
    if [[ "$index" == *"$SUFFIX" ]]; then
      fail "'$index' is the temporary copy of a rebuild; its write block is what keeps writes from being lost."
    fi
    aliases "$index" >/dev/null || exit 1
    if exists "$index$SUFFIX"; then
      fail "'$index$SUFFIX' exists, so a rebuild of '$index' is part way. Rerun '$SELF rebuild $index', or abandon it with '$SELF delete-index $index$SUFFIX' first."
    fi
    es PUT "/$index/_settings" '{"index.blocks.write":null}' >/dev/null || exit 1
    echo "Removed the write block from '$index'."
    ;;
  delete-index)
    [ $# -eq 2 ] || usage
    index="$2"
    require_name "$index" "$INDEX_NAME" index
    # The lookup must succeed and return this one index: a failed lookup is not "no aliases".
    current="$(aliases "$index")" || exit 1
    if [[ "$index" == *"$SUFFIX" ]]; then
      original="${index%"$SUFFIX"}"
      [ "$current" = "{}" ] || fail "'$index' has aliases $current: it is serving a rebuild. Rerun '$SELF rebuild $original'."
      stop_if_reindexing "$index" "$original"
      exists "$original" || fail "'$index' holds the only complete copy of '$original'. Rerun '$SELF rebuild $original'."
      from="$(count "$index")" || exit 1
      to="$(count "$original")" || exit 1
      [ "$to" -ge "$from" ] || fail "'$original' has $to document(s) but '$index' has $from, so '$index' is the complete copy. Rerun '$SELF rebuild $original'."
    elif [ "$current" != "{}" ]; then
      fail "'$index' has aliases $current; rebuild it instead."
    fi
    es DELETE "/$index" >/dev/null || exit 1
    echo "Deleted '$index'."
    ;;
  delete-data-stream)
    [ $# -eq 2 ] || usage
    name="$2"
    require_name "$name" "$DATA_STREAM_NAME" "data stream"
    es DELETE "/_data_stream/$name" >/dev/null || exit 1
    echo "Deleted data stream '$name'."
    ;;
  system-features)
    [[ "$SYSTEM_FEATURES_TIMEOUT" =~ ^[0-9]+$ ]] || fail "SYSTEM_FEATURES_TIMEOUT must be a number of seconds."
    es POST /_migration/system_features || exit 1
    deadline=$((SECONDS + SYSTEM_FEATURES_TIMEOUT))
    while true; do
      state=""
      if response="$(es GET /_migration/system_features)"; then
        # Prints the overall status, then one line for each feature that is not finished.
        report="$(python3 -c '
import json, sys
found = json.load(sys.stdin)
print(found.get("migration_status", ""))
for feature in found.get("features", []):
    name, status = feature.get("feature_name"), feature.get("migration_status")
    if status != "NO_MIGRATION_NEEDED":
        print(f"  {name}: {status}")
        for index in feature.get("indices", []):
            cause = index.get("failure_cause")
            if cause:
                print("    " + str(index.get("index")) + ": " + json.dumps(cause))' <<<"$response")" || exit 1
        state="${report%%$'\n'*}"
        features="${report#"$state"}"
        case "$state" in
          NO_MIGRATION_NEEDED)
            echo "System features migrated."
            break
            ;;
          MIGRATION_NEEDED | IN_PROGRESS)
            echo "$state$features"
            ;;
          ERROR)
            fail "the system feature migration failed. Fix the cause, then rerun '$SELF system-features'.$features"
            ;;
          *)
            fail "unexpected system feature migration status '$state':
$response"
            ;;
        esac
      else
        echo "WARNING: could not read the migration status; trying again until the deadline." >&2
      fi
      if [ "$SECONDS" -ge "$deadline" ]; then
        fail "system features were not migrated within ${SYSTEM_FEATURES_TIMEOUT}s (last status: ${state:-unknown}). The migration may still be running: check again with '$SELF check', or rerun with a larger SYSTEM_FEATURES_TIMEOUT."
      fi
      sleep 10
    done
    ;;
  *)
    usage
    ;;
esac
