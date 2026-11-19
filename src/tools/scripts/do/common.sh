#!/usr/bin/env bash

die() { printf 'ERROR: %s\n' "$*" >&2; exit 1; }

# Bash getopts works on macOS Bash 3.2 and Git Bash as well as Linux. Short
# options may be separate, attached, or grouped; long options use --name value
# or --name=value. Validate each command's supported options before any work.
parse_options() {
  n='' p='' v='' y='' e='' r='' b='' f='' d='' t='' c='' i='' l='' m='' s=''
  from='' to='' output='' offline='' option_keys=''
  local OPTIND=1 OPTARG option key value takes_value long_name
  local spec=':hn:p:vye:b:f:dt:c:i:l:s:o:-:'
  # These historical short names have command-specific meanings.
  if [[ "$action" == kafka-update ]]; then spec+='r'; else spec+='r:'; fi
  if [[ "$group" == oc && "$action" == release ]]; then spec+='m'; else spec+='m:'; fi
  while getopts "$spec" option; do
    key=$option; value=${OPTARG:-}; takes_value=true; long_name=''
    case "$option" in
      h) command_help; exit 0 ;;
      v|y|d) value=1; takes_value=false ;;
      r) [[ "$action" != kafka-update ]] || { value=1; takes_value=false; } ;;
      m) [[ "$group:$action" != oc:release ]] || { value=1; takes_value=false; } ;;
      o) key=output ;;
      -)
        long_name=${OPTARG%%=*}
        case "$long_name" in
          help) [[ "$OPTARG" != *=* ]] || die '--help does not take a value'; command_help; exit 0 ;;
          from|to|output) key=$long_name ;;
          offline) key=offline; takes_value=false ;;
          yes) key=y; takes_value=false ;;
          dry-run) key=d; takes_value=false ;;
          volumes) key=v; takes_value=false ;;
          *) die "Unknown option '--$long_name'" ;;
        esac
        if [[ "$takes_value" == false ]]; then
          [[ "$OPTARG" != *=* ]] || die "--$long_name does not take a value"
          value=1
        elif [[ "$OPTARG" == *=* ]]; then
          value=${OPTARG#*=}
        else
          [[ $OPTIND -le $# ]] || die "--$long_name requires a value"
          value=${!OPTIND}; OPTIND=$((OPTIND + 1))
        fi ;;
      :) die "Option '-$OPTARG' requires a value" ;;
      \?) die "Unknown option '-$OPTARG'" ;;
    esac
    if [[ "$takes_value" == true && ( -z "$value" || "$value" == -* ) ]]; then
      if [[ -n "$long_name" ]]; then die "--$long_name requires a value"; else die "Option '-$option' requires a value"; fi
    fi
    printf -v "$key" '%s' "$value"
    option_keys="$option_keys $key"
    long_name=''
  done
  shift "$((OPTIND - 1))"
  [[ $# == 0 ]] || die "Unexpected argument '$1'. Use flags such as '-n api', not 'n=api'."
}

command_help() {
  case "$action" in db-*) do_help db ;; *) do_help "$group" ;; esac
}

allow_options() {
  local key flag
  for key in $option_keys; do
    case "$key" in from|to|output|offline) flag="--$key" ;; *) flag="-$key" ;; esac
    case " $* " in *" $key "*) ;; *) die "Option '$flag' is not supported by '$group $action'" ;; esac
  done
}

oc_script() (
  cd "$DO_ROOT/openshift/scripts" || exit
  bash "$@"
)
