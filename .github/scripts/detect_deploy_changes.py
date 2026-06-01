#!/usr/bin/env python3
import argparse
import json
from pathlib import Path


BUILD_TARGETS = [
    {"name": "api", "image": "api", "dockerfile": "api/net/Dockerfile", "context": "."},
    {"name": "charts-api", "image": "charts-api", "dockerfile": "api/node/Dockerfile", "context": "api/node"},
    {
        "name": "editor",
        "image": "editor",
        "dockerfile": "app/editor/Dockerfile.nginx",
        "context": "./app/editor",
        "build_arg_name": "REACT_APP_VERSION",
    },
    {
        "name": "subscriber",
        "image": "subscriber",
        "dockerfile": "app/subscriber/Dockerfile.nginx",
        "context": "./app/subscriber",
        "build_arg_name": "REACT_APP_VERSION",
    },
    {"name": "content-service", "image": "content-service", "dockerfile": "services/net/content/Dockerfile", "context": "."},
    {"name": "indexing-service", "image": "indexing-service", "dockerfile": "services/net/indexing/Dockerfile", "context": "."},
    {
        "name": "transcription-service",
        "image": "transcription-service",
        "dockerfile": "services/net/transcription/Dockerfile",
        "context": ".",
    },
    {"name": "nlp-service", "image": "nlp-service", "dockerfile": "services/net/nlp/Dockerfile", "context": "."},
    {
        "name": "extract-quotes-service",
        "image": "extract-quotes-service",
        "dockerfile": "services/net/extract-quotes/Dockerfile",
        "context": ".",
    },
    {"name": "reporting-service", "image": "reporting-service", "dockerfile": "services/net/reporting/Dockerfile", "context": "."},
    {
        "name": "notification-service",
        "image": "notification-service",
        "dockerfile": "services/net/notification/Dockerfile",
        "context": ".",
    },
    {"name": "scheduler-service", "image": "scheduler-service", "dockerfile": "services/net/scheduler/Dockerfile", "context": "."},
    {"name": "ffmpeg-service", "image": "ffmpeg-service", "dockerfile": "services/net/ffmpeg/Dockerfile", "context": "."},
    {
        "name": "event-handler-service",
        "image": "event-handler-service",
        "dockerfile": "services/net/event-handler/Dockerfile",
        "context": ".",
    },
    {
        "name": "ches-retry-service",
        "image": "ches-retry-service",
        "dockerfile": "services/net/ches-retry/Dockerfile",
        "context": ".",
    },
    {
        "name": "syndication-service",
        "image": "syndication-service",
        "dockerfile": "services/net/syndication/Dockerfile",
        "context": ".",
    },
    {"name": "filemonitor-service", "image": "filemonitor-service", "dockerfile": "services/net/filemonitor/Dockerfile", "context": "."},
    {"name": "image-service", "image": "image-service", "dockerfile": "services/net/image/Dockerfile", "context": "."},
    {
        "name": "auto-clipper-service",
        "image": "auto-clipper-service",
        "dockerfile": "services/net/auto-clipper/Dockerfile",
        "context": ".",
    },
    {
        "name": "contentmigration-service",
        "image": "contentmigration-service",
        "dockerfile": "services/net/contentmigration/Dockerfile",
        "context": ".",
    },
    {
        "name": "folder-collection-service",
        "image": "folder-collection-service",
        "dockerfile": "services/net/folder-collection/Dockerfile",
        "context": ".",
    },
]

SERVICE_TARGETS = [
    {"name": "charts-api", "deployment": "charts-api"},
    {"name": "content-service", "deployment": "content-service"},
    {"name": "indexing-service", "deployment": "indexing-service"},
    {"name": "transcription-service", "deployment": "transcription-service"},
    {"name": "nlp-service", "deployment": "nlp-service"},
    {"name": "extract-quotes-service", "deployment": "extract-quotes-service"},
    {"name": "reporting-service", "deployment": "reporting-service"},
    {"name": "notification-service", "deployment": "notification-service"},
    {"name": "scheduler-service", "deployment": "scheduler-service"},
    {"name": "ffmpeg-service", "deployment": "ffmpeg-service"},
    {"name": "event-handler-service", "deployment": "event-handler-service"},
    {"name": "ches-retry-service", "deployment": "ches-retry-service"},
    {"name": "syndication-service", "deployment": "syndication-service"},
    {"name": "filemonitor-service", "deployment": "filemonitor-service"},
    {"name": "image-service", "deployment": "image-service"},
    {"name": "auto-clipper-service", "deployment": "auto-clipper-service"},
    {"name": "contentmigration-service", "deployment": "contentmigration-service"},
    {"name": "folder-collection-service", "deployment": "folder-collection-service"},
]

FRONTEND_TARGETS = [
    {"name": "editor", "deployment": "editor"},
    {"name": "subscriber", "deployment": "subscriber"},
]

DOTNET_SERVICE_NAMES = [
    "content-service",
    "indexing-service",
    "transcription-service",
    "nlp-service",
    "extract-quotes-service",
    "reporting-service",
    "notification-service",
    "scheduler-service",
    "ffmpeg-service",
    "event-handler-service",
    "ches-retry-service",
    "syndication-service",
    "filemonitor-service",
    "image-service",
    "auto-clipper-service",
    "contentmigration-service",
    "folder-collection-service",
]

SERVICE_PATHS = {
    "services/net/content/": "content-service",
    "services/net/indexing/": "indexing-service",
    "services/net/transcription/": "transcription-service",
    "services/net/nlp/": "nlp-service",
    "services/net/extract-quotes/": "extract-quotes-service",
    "services/net/reporting/": "reporting-service",
    "services/net/notification/": "notification-service",
    "services/net/scheduler/": "scheduler-service",
    "services/net/ffmpeg/": "ffmpeg-service",
    "services/net/event-handler/": "event-handler-service",
    "services/net/ches-retry/": "ches-retry-service",
    "services/net/syndication/": "syndication-service",
    "services/net/filemonitor/": "filemonitor-service",
    "services/net/image/": "image-service",
    "services/net/auto-clipper/": "auto-clipper-service",
    "services/net/contentmigration/": "contentmigration-service",
    "services/net/folder-collection/": "folder-collection-service",
}

KUSTOMIZE_SERVICE_PATHS = {
    "openshift/kustomize/services/content/": "content-service",
    "openshift/kustomize/services/indexing/": "indexing-service",
    "openshift/kustomize/services/transcription/": "transcription-service",
    "openshift/kustomize/services/nlp/": "nlp-service",
    "openshift/kustomize/services/extract-quotes/": "extract-quotes-service",
    "openshift/kustomize/services/reporting/": "reporting-service",
    "openshift/kustomize/services/notification/": "notification-service",
    "openshift/kustomize/services/scheduler/": "scheduler-service",
    "openshift/kustomize/services/ffmpeg/": "ffmpeg-service",
    "openshift/kustomize/services/event-handler/": "event-handler-service",
    "openshift/kustomize/services/ches-retry/": "ches-retry-service",
    "openshift/kustomize/services/syndication/": "syndication-service",
    "openshift/kustomize/services/filemonitor/": "filemonitor-service",
    "openshift/kustomize/services/image/": "image-service",
    "openshift/kustomize/services/auto-clipper/": "auto-clipper-service",
    "openshift/kustomize/services/contentmigration-current/": "contentmigration-service",
    "openshift/kustomize/services/contentmigration-historic/": "contentmigration-service",
    "openshift/kustomize/services/folder-collection/": "folder-collection-service",
}

BUILD_BY_NAME = {target["name"]: target for target in BUILD_TARGETS}
SERVICE_BY_NAME = {target["name"]: target for target in SERVICE_TARGETS}
FRONTEND_BY_NAME = {target["name"]: target for target in FRONTEND_TARGETS}


def normalize_path(path):
    return path.replace("\\", "/").lstrip("./")


def matrix_from_names(targets_by_name, names):
    ordered = [target for name, target in targets_by_name.items() if name in names]
    return {"include": ordered}


def detect_deploy_changes(changed_files, scope):
    if scope not in {"all", "changed"}:
        raise ValueError(f"Unsupported deploy scope: {scope}")

    if scope == "all":
        return {
            "build_matrix": {"include": BUILD_TARGETS},
            "service_matrix": {"include": SERVICE_TARGETS},
            "frontend_matrix": {"include": FRONTEND_TARGETS},
            "has_builds": True,
            "deploy_api": True,
            "deploy_services": True,
            "deploy_frontend": True,
            "run_db_migration": True,
        }

    build_names = set()
    service_names = set()
    frontend_names = set()
    deploy_api = False
    run_db_migration = False

    for raw_path in changed_files:
        path = normalize_path(raw_path)

        if path.startswith("api/net/"):
            build_names.add("api")
            deploy_api = True
        elif path.startswith("api/node/") or path.startswith("openshift/kustomize/charts/"):
            build_names.add("charts-api")
            service_names.add("charts-api")
        elif path.startswith("app/editor/") or path.startswith("openshift/kustomize/app/editor/"):
            build_names.add("editor")
            frontend_names.add("editor")
        elif path.startswith("app/subscriber/") or path.startswith("openshift/kustomize/app/subscriber/"):
            build_names.add("subscriber")
            frontend_names.add("subscriber")
        elif path.startswith("libs/net/"):
            build_names.add("api")
            build_names.update(DOTNET_SERVICE_NAMES)
            service_names.update(DOTNET_SERVICE_NAMES)
            deploy_api = True
            if (
                path.startswith("libs/net/dal/")
                or path.startswith("libs/net/entities/")
                or path == "libs/net/Dockerfile"
            ):
                run_db_migration = True
        elif path.startswith("db/postgres/"):
            run_db_migration = True
        elif path.startswith("openshift/kustomize/api/") or path.startswith("openshift/kustomize/api-services/"):
            deploy_api = True
        elif path.startswith("openshift/kustomize/shared_resources/"):
            build_names.add("api")
            build_names.update(DOTNET_SERVICE_NAMES)
            service_names.update(DOTNET_SERVICE_NAMES)
            frontend_names.update({"editor", "subscriber"})
            deploy_api = True

        for prefix, service_name in SERVICE_PATHS.items():
            if path.startswith(prefix):
                build_names.add(service_name)
                service_names.add(service_name)

        for prefix, service_name in KUSTOMIZE_SERVICE_PATHS.items():
            if path.startswith(prefix):
                service_names.add(service_name)

    build_matrix = matrix_from_names(BUILD_BY_NAME, build_names)
    service_matrix = matrix_from_names(SERVICE_BY_NAME, service_names)
    frontend_matrix = matrix_from_names(FRONTEND_BY_NAME, frontend_names)

    return {
        "build_matrix": build_matrix,
        "service_matrix": service_matrix,
        "frontend_matrix": frontend_matrix,
        "has_builds": len(build_matrix["include"]) > 0,
        "deploy_api": deploy_api,
        "deploy_services": len(service_matrix["include"]) > 0,
        "deploy_frontend": len(frontend_matrix["include"]) > 0,
        "run_db_migration": run_db_migration,
    }


def write_github_output(path, result):
    lines = []
    for key, value in result.items():
        if key.endswith("_matrix"):
            rendered = json.dumps(value, separators=(",", ":"))
        elif isinstance(value, bool):
            rendered = str(value).lower()
        else:
            rendered = str(value)
        lines.append(f"{key}={rendered}")
    with open(path, "a", encoding="utf-8") as output:
        output.write("\n".join(lines))
        output.write("\n")


def parse_args():
    parser = argparse.ArgumentParser(description="Detect deploy-all targets from changed files.")
    parser.add_argument("--scope", choices=["all", "changed"], required=True)
    parser.add_argument("--changed-files", default="")
    parser.add_argument("--github-output", default="")
    return parser.parse_args()


def main():
    args = parse_args()
    changed_files = []
    if args.changed_files:
        changed_files = Path(args.changed_files).read_text(encoding="utf-8").splitlines()

    result = detect_deploy_changes(changed_files, args.scope)
    if args.github_output:
        write_github_output(args.github_output, result)
    else:
        print(json.dumps(result, indent=2))


if __name__ == "__main__":
    main()
