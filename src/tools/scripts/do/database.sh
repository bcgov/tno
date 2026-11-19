#!/usr/bin/env bash

# One EF implementation for the host and SDK containers. Options are validated
# before building an image or running a command that can change the database.
database_action() {
  local command=$1
  shift
  case "$command" in
    db-migrations) allow_options offline ;;
    db-add) allow_options n; [[ -n "$n" ]] || die 'db-add requires -n <migration name>' ;;
    db-apply) allow_options n ;;
    db-rollback) allow_options n; [[ -n "$n" ]] || die 'db-rollback requires -n <migration or 0>' ;;
    db-remove|db-clean|db-refresh) allow_options ;;
    db-drop) allow_options y ;;
    db-script) allow_options from to output ;;
    db-recreate) allow_options r n ;;
    *) die "Unknown database command '$command'. Run 'bash do help db'." ;;
  esac
  case "$command" in
    db-add|db-recreate)
      [[ "${n:-Initial}" =~ ^[a-zA-Z0-9_][a-zA-Z0-9_.-]*$ ]] || die 'Migration names may contain letters, digits, underscores, dots and hyphens.' ;;
    db-script)
      output=${output:-db-migration.sql}
      output=${output//\\//}
      case "$output" in /*|[a-zA-Z]:*|..|../*|*/../*|*/..) die 'output must be a path inside the repository, relative to its root.' ;; esac ;;
  esac
  if [[ "$group" == docker || "$group" == podman ]]; then
    database_container "$command" "$@"
  else
    database_local "$command"
  fi
}

database_container() {
  local engine=$group root=$DO_ROOT project key build_args=()
  local args=(run --rm -i --workdir /workspace -e DOTNET_CLI_HOME=/tmp
    -e NUGET_PACKAGES=/tmp/nuget)
  [[ ! -t 0 || ! -t 1 ]] || args+=(-t)
  case "${OSTYPE:-}" in
    msys*|cygwin*) root=$(cygpath -m "$root") ;;
    *) args+=(--user "$(id -u):$(id -g)")
       build_args+=(--build-arg "DB_TOOLS_UID=$(id -u)" --build-arg "DB_TOOLS_GID=$(id -g)") ;;
  esac
  args+=(--mount "type=bind,source=$root,target=/workspace")
  # Hide host build products (including Windows-generated assets) for every
  # library project. Keep generated migrations/SQL on the writable bind mount.
  for project in "$DO_ROOT"/src/libs/*/*.csproj; do
    [[ -f "$project" ]] || continue
    project=${project%/*}; project=${project#"$DO_ROOT"/}
    args+=(--tmpfs "/workspace/$project/obj:rw,mode=1777"
      --tmpfs "/workspace/$project/bin:rw,mode=1777")
  done
  if [[ "${DB_NETWORK:-tno-net}" != none ]]; then
    args+=(--network "${DB_NETWORK:-tno-net}")
  fi
  args+=(--add-host host.docker.internal:host-gateway)
  for key in ConnectionStrings__TNO DB_POSTGRES_USERNAME DB_POSTGRES_PASSWORD ASPNETCORE_ENVIRONMENT; do
    [[ -z "${!key+x}" ]] || args+=(-e "$key")
  done
  "$engine" build "${build_args[@]}" -t tno:db-tools -f src/tools/scripts/do/Dockerfile.db src/tools/scripts/do
  "$engine" "${args[@]}" tno:db-tools bash do "$@"
}

database_local() (
  local command=$1 args=() path
  command -v dotnet >/dev/null 2>&1 || die 'Install the .NET 10 SDK, or use bash do docker db-*.'
  dotnet ef --version >/dev/null 2>&1 || die 'Install dotnet-ef 10.0.14 (dotnet tool install --global dotnet-ef --version 10.0.14), or use bash do docker db-*.'
  cd "$DO_ROOT/src/libs/dal" || exit
  # EF 10 needs restored assets to inspect project metadata on a clean checkout.
  dotnet restore
  case "$command" in
    db-migrations)
      [[ -z "$offline" ]] || args+=(--no-connect)
      dotnet ef migrations list "${args[@]}" ;;
    db-add)
      dotnet ef migrations add "$n"
      bash "$DO_ROOT/src/tools/scripts/do/seed-migration.sh" "$n" ;;
    db-apply|db-rollback)
      [[ -z "$n" ]] || args+=("$n")
      dotnet ef database update "${args[@]}" ;;
    db-remove) dotnet ef migrations remove --force ;;
    db-drop)
      [[ -z "$y" ]] || args+=(--force)
      dotnet ef database drop "${args[@]}" ;;
    db-refresh)
      dotnet ef database update 0
      dotnet ef database update ;;
    db-clean)
      dotnet ef database drop --force
      dotnet ef database update ;;
    db-script)
      args=("${from:-0}")
      [[ -z "$to" ]] || args+=("$to")
      path="$DO_ROOT/$output"
      mkdir -p "$(dirname "$path")"
      case "${OSTYPE:-}" in msys*|cygwin*) path=$(cygpath -m "$path") ;; esac
      dotnet ef migrations script "${args[@]}" --output "$path" ;;
    db-recreate)
      dotnet ef database update "${r:-0}"
      dotnet ef migrations remove --force
      n=${n:-Initial}
      dotnet ef migrations add "$n"
      bash "$DO_ROOT/src/tools/scripts/do/seed-migration.sh" "$n"
      dotnet ef database update ;;
  esac
)
