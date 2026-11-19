#!/usr/bin/env python3
"""Move local environment files left behind when checking out the new layout."""

from pathlib import Path
import os
import re
import shutil
import sys


SOURCE_FOLDERS = ("playwright-automation", "api", "app", "libs", "services", "tools")
DEPENDENCY_FOLDERS = ("db", "auth", "network")
LEGACY_LOCATIONS = {
    "src/libs/net": "src/libs",
    "src/libs/tests": "src/tests",
    "src/services/net": "src/services",
    "src/api/node": "src/charts-api",
    "src/api/net": "src/api",
    "dependencies/kafka/kafka-ui": "dependencies/kafka-ui",
}
SKIP_FOLDERS = {"node_modules", ".git", ".worktrees", "bin", "obj", "__pycache__"}


def migrate(root: Path) -> int:
    """Preflight all destinations, then move files without replacing local settings."""
    moves = []
    destinations = {}
    for folder in (*LEGACY_LOCATIONS, *SOURCE_FOLDERS, *DEPENDENCY_FOLDERS):
        source = root / folder
        target = root / "src" / folder if folder in SOURCE_FOLDERS else root / "dependencies"
        if folder in LEGACY_LOCATIONS:
            target = root / LEGACY_LOCATIONS[folder]
        if not source.is_dir():
            continue
        for directory, folders, files in os.walk(source):
            folders[:] = sorted(
                name for name in folders
                if name not in SKIP_FOLDERS
                and Path(directory, name) not in (root / "db/kafka/kowl", root / "libs/npm")
            )
            for name in sorted(files):
                if not (name == ".env" or name.startswith(".env.") or name.endswith(".env")):
                    continue
                old = Path(directory) / name
                new = target / old.relative_to(source)
                # Resolve environments from the original root layout in one pass.
                for previous, current in LEGACY_LOCATIONS.items():
                    if new.is_relative_to(root / previous):
                        new = root / current / new.relative_to(root / previous)
                if new.exists() or new.is_symlink():
                    if old.is_file() and new.is_file() and old.read_bytes() == new.read_bytes():
                        print(f"Already present: {new.relative_to(root)} (original retained)")
                        continue
                    raise ValueError(f"Destination already exists: {new.relative_to(root)}; resolve it before rerunning bash do move. No files moved.")
                if new in destinations:
                    raise ValueError(f"Multiple environment files map to {new.relative_to(root)}. No files moved.")
                destinations[new] = old
                moves.append((old, new))

    for old, new in moves:
        new.parent.mkdir(parents=True, exist_ok=True)
        shutil.move(str(old), str(new))
        print(f"Moved {old.relative_to(root)} -> {new.relative_to(root)}")

    # Compose reads these optional build-context overrides from the root .env.
    # Preserve every other setting, including secrets, quotes and line endings.
    env = root / ".env"
    if env.is_file():
        original = env.read_bytes()
        updated = re.sub(
            rb"(?m)^((?:export[ \t]+)?(?:DB_CONTEXT|ORACLE_DB_CONTEXT)[ \t]*=[ \t]*['\"]?)(\./)?db/",
            rb"\1\2dependencies/",
            original,
        )
        if original != updated:
            env.write_bytes(updated)
            print("Updated database build-context paths in root .env")
    print(f"Moved {len(moves)} environment file(s). Root .env stays at the repository root.")
    return len(moves)


if __name__ == "__main__":
    try:
        migrate(Path(__file__).resolve().parents[3])
    except (OSError, ValueError) as error:
        print(f"Environment migration failed: {error}", file=sys.stderr)
        sys.exit(1)
