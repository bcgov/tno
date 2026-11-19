#!/usr/bin/env bash
# Apply TNO's seed hooks only to a newly generated migration, from the DAL cwd.
set -eo pipefail
name=${1:?Migration name required}
files=()
for file in Migrations/*_"$name".cs; do
  [[ ! -f "$file" ]] || files+=("$file")
done
[[ ${#files[@]} == 1 ]] || { echo "Expected exactly one new migration named '$name'." >&2; exit 1; }
file=${files[0]}
# Refuse an unexpected/already transformed file rather than silently corrupting it.
grep -q ': Migration' "$file" || { echo 'Expected an unmodified EF migration.' >&2; exit 1; }
temporary=$(mktemp "${file}.XXXXXX")
trap 'rm -f "$temporary"' EXIT
# EF emits method braces at the same indentation. Match that boundary rather
# than counting braces inside SQL strings or nested object initializers.
LC_ALL=C awk '
  NR == 1 {sub(/^\357\273\277/, "")}
  {sub(/\r$/, "")}
  BEGIN {print "using TNO.DAL;"}
  /: Migration/ {sub(/: Migration/, ": SeedMigration")}
  /protected override void (Up|Down)\(MigrationBuilder migrationBuilder\)/ {
    method = ($0 ~ /void Up/) ? "Up" : "Down"; waiting = 1
  }
  {
    line = $0
    if (waiting && line ~ /^[[:space:]]*\{[[:space:]]*$/) {
      print line
      method_indent = line; sub(/\{.*/, "", method_indent); indent = method_indent "    "
      print indent "Pre" method "(migrationBuilder);"
      waiting = 0; active = 1; next
    }
    if (active) {
      if (line == method_indent "}") {print indent "Post" method "(migrationBuilder);"; active = 0; completed++}
    }
    print line
  }
  END {if (completed != 2) exit 1}
' "$file" > "$temporary"
cat "$temporary" > "$file"
