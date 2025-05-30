"""
Settings loader for S3 downloader.
"""

import sys
from pathlib import Path
from typing import Any, Callable, Dict

from .default_settings import (
    DATABASE_SETTINGS,
    DISK_SPACE_SETTINGS,
    DOWNLOADER_BEHAVIOR_SETTINGS,
    ERROR_HANDLING_SETTINGS,
    FILTER_SETTINGS,
    S3_SETTINGS,
    SCHEDULER_SETTINGS,
    STORAGE_SETTINGS,
)


def get_resource_path(relative_path: str) -> Path:
    """
    Get absolute path to resource, works for dev and for PyInstaller.

    Args:
        relative_path: Relative path to resource

    Returns:
        Absolute path to resource
    """
    try:
        # PyInstaller creates a temp folder and stores path in _MEIPASS
        base_path = Path(sys._MEIPASS)  # type: ignore
    except AttributeError:
        # Running in development mode
        base_path = Path(__file__).parent.parent.parent

    return base_path / relative_path


def load_embedded_env_vars() -> Dict[str, str]:
    """
    Load environment variables from embedded .env file.

    Returns:
        Dictionary of environment variables

    Raises:
        RuntimeError: If .env file cannot be loaded
    """
    env_vars = {}

    try:
        env_file_path = get_resource_path(".env")
        if not env_file_path.exists():
            raise RuntimeError(f"Embedded .env file not found at {env_file_path}")

        with open(env_file_path, "r", encoding="utf-8") as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith("#") and "=" in line:
                    key, value = line.split("=", 1)
                    env_vars[key.strip()] = value.strip()

        if not env_vars:
            raise RuntimeError("No valid environment variables found in .env file")

        print(f"Loaded {len(env_vars)} environment variables from embedded .env file")
        return env_vars

    except Exception as e:
        if isinstance(e, RuntimeError):
            raise
        raise RuntimeError(f"Error loading embedded .env file: {e}") from e


class Settings:
    """Settings class for S3 downloader."""

    def __init__(self):
        """Initialize settings."""
        # Load default settings
        self.s3 = S3_SETTINGS.copy()
        self.storage = STORAGE_SETTINGS.copy()
        self.database = DATABASE_SETTINGS.copy()
        self.scheduler = SCHEDULER_SETTINGS.copy()
        self.downloader_behavior = DOWNLOADER_BEHAVIOR_SETTINGS.copy()
        self.error_handling = ERROR_HANDLING_SETTINGS.copy()
        self.disk_space = DISK_SPACE_SETTINGS.copy()
        self.filter = FILTER_SETTINGS.copy()

    def load_from_env(self) -> None:
        """Load settings from environment variables."""
        # Load environment variables from embedded .env file or system
        env_vars = load_embedded_env_vars()

        # Override S3 settings
        self._override_from_env_dict(env_vars, self.s3, "S3_SERVICE_URL", "SERVICE_URL", str)
        self._override_from_env_dict(env_vars, self.s3, "S3_BUCKET_NAME", "BUCKET_NAME", str)
        self._override_from_env_dict(env_vars, self.s3, "S3_ACCESS_KEY", "ACCESS_KEY", str)
        self._override_from_env_dict(env_vars, self.s3, "S3_SECRET_KEY", "SECRET_KEY", str)
        self._override_from_env_dict(env_vars, self.s3, "S3_TIMEOUT", "TIMEOUT", int)

        # Override storage settings
        self._override_from_env_dict(env_vars, self.storage, "LOCAL_STORAGE_PATH", "PATH", str)

        # Override database settings
        self._override_from_env_dict(env_vars, self.database, "DATABASE_URL", "URL", str)

        # Override scheduler settings
        self._override_from_env_dict(
            env_vars, self.scheduler, "SCHEDULER_INTERVAL", "INTERVAL", int
        )

        # Override downloader behavior settings
        self._override_from_env_dict(
            env_vars, self.downloader_behavior, "MAX_FILES_PER_TASK", "MAX_FILES_PER_TASK", int
        )

        # Override error handling settings
        self._override_from_env_dict(
            env_vars,
            self.error_handling,
            "MAX_CONSECUTIVE_FAILURES",
            "MAX_CONSECUTIVE_FAILURES",
            int,
        )
        self._override_from_env_dict(
            env_vars, self.error_handling, "MAX_FAILURE_PERCENTAGE", "MAX_FAILURE_PERCENTAGE", float
        )
        self._override_from_env_dict(
            env_vars, self.error_handling, "NETWORK_TEST_INTERVAL", "NETWORK_TEST_INTERVAL", int
        )

        # Override disk space settings
        self._override_from_env_dict(
            env_vars, self.disk_space, "SPACE_WARNING_THRESHOLD", "WARNING_THRESHOLD", float
        )

        # Override filter settings
        self._override_exclude_prefixes_from_env_dict(env_vars)

    def _override_from_env_dict(
        self,
        env_vars: Dict[str, str],
        settings_dict: Dict[str, Any],
        env_key: str,
        settings_key: str,
        type_converter: Callable[[str], Any],
    ) -> None:
        """
        Override settings value from environment variables dictionary.

        Args:
            env_vars: Environment variables dictionary
            settings_dict: Settings dictionary to update
            env_key: Environment variable key
            settings_key: Settings key
            type_converter: Function to convert string to appropriate type
        """
        env_value = env_vars.get(env_key)
        if env_value is not None:
            try:
                settings_dict[settings_key] = type_converter(env_value)
            except (ValueError, TypeError) as e:
                print(f"Error converting {env_key} value '{env_value}': {e}")

    def _override_exclude_prefixes_from_env_dict(self, env_vars: Dict[str, str]) -> None:
        """Override exclude prefixes from environment variables dictionary."""
        exclude_prefixes = env_vars.get("EXCLUDE_PREFIXES")
        if exclude_prefixes:
            try:
                # Remove quotes if present
                exclude_prefixes = exclude_prefixes.strip("\"'")
                self.filter["EXCLUDE_PREFIXES"] = [
                    prefix.strip() for prefix in exclude_prefixes.split(",")
                ]
            except ValueError as e:
                print(f"Error parsing EXCLUDE_PREFIXES: {e}")


# Create a singleton instance
settings = Settings()
