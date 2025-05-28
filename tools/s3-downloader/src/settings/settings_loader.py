"""
Settings loader for S3 downloader.
"""

import os
from typing import Any, Callable, Dict

from dotenv import load_dotenv

from .default_settings import (
    DATABASE_SETTINGS,
    DISK_SPACE_SETTINGS,
    DOWNLOADER_BEHAVIOR_SETTINGS,
    ERROR_HANDLING_SETTINGS,
    S3_SETTINGS,
    SCHEDULER_SETTINGS,
    STORAGE_SETTINGS,
)


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

    def load_from_env(self) -> None:
        """Load settings from environment variables."""
        # Load .env file
        load_dotenv()

        # Override S3 settings
        self._override_from_env(self.s3, "S3_SERVICE_URL", "SERVICE_URL", str)
        self._override_from_env(self.s3, "S3_BUCKET_NAME", "BUCKET_NAME", str)
        self._override_from_env(self.s3, "S3_ACCESS_KEY", "ACCESS_KEY", str)
        self._override_from_env(self.s3, "S3_SECRET_KEY", "SECRET_KEY", str)
        self._override_from_env(self.s3, "S3_TIMEOUT", "TIMEOUT", int)

        # Override storage settings
        self._override_from_env(self.storage, "LOCAL_STORAGE_PATH", "PATH", str)

        # Override database settings
        self._override_from_env(self.database, "DATABASE_URL", "URL", str)

        # Override scheduler settings
        self._override_from_env(self.scheduler, "SCHEDULER_INTERVAL", "INTERVAL", int)

        # Override downloader behavior settings
        self._override_from_env(
            self.downloader_behavior, "MAX_FILES_PER_TASK", "MAX_FILES_PER_TASK", int
        )

        # Override error handling settings
        self._override_from_env(
            self.error_handling, "MAX_CONSECUTIVE_FAILURES", "MAX_CONSECUTIVE_FAILURES", int
        )
        self._override_from_env(
            self.error_handling, "MAX_FAILURE_PERCENTAGE", "MAX_FAILURE_PERCENTAGE", float
        )
        self._override_from_env(
            self.error_handling, "NETWORK_TEST_INTERVAL", "NETWORK_TEST_INTERVAL", int
        )

        # Override disk space settings
        self._override_from_env(
            self.disk_space, "SPACE_WARNING_THRESHOLD", "WARNING_THRESHOLD", float
        )

    def _override_from_env(
        self,
        settings_dict: Dict[str, Any],
        env_key: str,
        settings_key: str,
        type_converter: Callable[[str], Any],
    ) -> None:
        """
        Override settings value from environment variable.

        Args:
            settings_dict: Settings dictionary to update
            env_key: Environment variable key
            settings_key: Settings key
            type_converter: Function to convert string to appropriate type
        """
        env_value = os.getenv(env_key)
        if env_value is not None:
            try:
                settings_dict[settings_key] = type_converter(env_value)
            except (ValueError, TypeError) as e:
                print(f"Error converting {env_key} value '{env_value}': {e}")


# Create a singleton instance
settings = Settings()
