#!/usr/bin/env bash

oc_action() {
  case "$action" in
    project) allow_options; oc project 9b301c-tools ;;
    switch) allow_options n; oc project "9b301c-${n:-tools}" ;;
    switch-current) allow_options n; bash src/tools/scripts/switch-project.sh "$n" ;;
    clean) allow_options n; oc delete pod --field-selector=status.phase==Succeeded -n "9b301c-${n:-dev}" ;;
    pod) allow_options n; oc_script get-pod-name.sh "$n" ;;
    logs) allow_options n i; oc_script get-logs.sh "$n" "${i:-0}" ;;
    rsh) allow_options l i; oc_script rsh.sh "$l" "${i:-0}" ;;
    port-forward) allow_options n i l r; oc_script port-forward.sh "$n" "${i:-0}" "$l" "$r" ;;
    db-connect) allow_options e l r; oc_script database-connect.sh "${e:-dev}" "${l:-22222}" "${r:-5432}" ;;
    release)
      allow_options e t n m d y
      local components=()
      # The old release target accepts a whitespace-separated component list.
      read -r -a components <<< "$n"
      MIGRATE="$m" DRY_RUN="$d" YES="$y" oc_script release.sh "${e:-dev}" "$t" "${components[@]}" ;;
    deploy) allow_options e t n m s p; oc_script deploy.sh "${e:-dev}" "${t:-latest}" "$n" "$m" "$s" "$p" ;;
    stop|up) allow_options e; oc_script "$action.sh" "${e:-dev}" ;;
    annotate) allow_options e n; oc_script annotate.sh "${e:-dev}" "$n" ;;
    build) allow_options n t e; oc_script build.sh "$n" "${t:-latest}" "${e:-prod}" ;;
    push|pull) allow_options n t; oc_script "$action.sh" "$n" "${t:-latest}" ;;
    tag) allow_options n f t r; oc_script tag.sh "$n" "${f:-latest}" "$t" "${r:-remote}" ;;
    kafka-topics)
      allow_options e
      oc exec -n "9b301c-${e:-dev}" kafka-broker-0 -- kafka-topics --describe --bootstrap-server kafka-headless:29092 |
        awk -F '\t' '/^Topic:/ && !/__consumer_offsets/ {print $1 "\t" $3 "\t" $4}' ;;
    kafka-*) kafka_action "$action" dev ;;
    *) die "Unknown oc command '$action'" ;;
  esac
}
