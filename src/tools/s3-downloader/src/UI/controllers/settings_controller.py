import logging

logger = logging.getLogger(__name__)


class SettingsController:
    """Controller for application settings."""

    def __init__(self):
        """Initialize the settings controller."""
        from src.settings import settings

        self.settings = settings

        # Log loaded settings for debugging
        self._log_current_settings()

    def _log_current_settings(self):
        """Log current settings for debugging."""
        try:
            logger.info(
                f"Loaded settings: "
                f"local_path={self.local_path}, "
                f"timeout={self.timeout}s, "
                f"scheduler_interval={self.scheduler_interval}s, "
                f"max_consecutive_failures={self.max_consecutive_failures}, "
                f"max_failure_percentage={self.max_failure_percentage}, "
                f"network_test_interval={self.network_test_interval}, "
                f"space_warning_threshold={self.space_warning_threshold}, "
                f"max_files_per_task={self.max_files_per_task}"
            )
        except Exception as e:
            logger.error(f"Error logging settings: {e}")

    # S3 connection settings properties
    @property
    def endpoint_url(self) -> str:
        return self.settings.s3["SERVICE_URL"]

    @property
    def bucket_name(self) -> str:
        return self.settings.s3["BUCKET_NAME"]

    @property
    def access_key(self) -> str:
        return self.settings.s3["ACCESS_KEY"]

    @property
    def secret_key(self) -> str:
        return self.settings.s3["SECRET_KEY"]

    @property
    def timeout(self) -> int:
        return self.settings.s3["TIMEOUT"]

    @property
    def s3_settings(self) -> dict:
        return self.settings.s3

    # Local storage settings
    @property
    def local_path(self) -> str:
        return self.settings.storage["PATH"]

    @property
    def storage_settings(self) -> dict:
        return self.settings.storage

    # Scheduler settings
    @property
    def scheduler_interval(self) -> int:
        return self.settings.scheduler["INTERVAL"]

    # Error handling settings
    @property
    def max_consecutive_failures(self) -> int:
        return self.settings.error_handling["MAX_CONSECUTIVE_FAILURES"]

    @property
    def max_failure_percentage(self) -> float:
        return self.settings.error_handling["MAX_FAILURE_PERCENTAGE"]

    @property
    def network_test_interval(self) -> int:
        return self.settings.error_handling["NETWORK_TEST_INTERVAL"]

    @property
    def error_handling_settings(self) -> dict:
        return self.settings.error_handling

    # Disk space settings
    @property
    def space_warning_threshold(self) -> float:
        return self.settings.disk_space["WARNING_THRESHOLD"]

    @property
    def disk_space_settings(self) -> dict:
        return self.settings.disk_space

    # Downloader behavior settings
    @property
    def max_files_per_task(self) -> int:
        try:
            return self.settings.downloader_behavior["MAX_FILES_PER_TASK"]
        except (AttributeError, KeyError):
            logger.warning("MAX_FILES_PER_TASK not found in settings, using default value 5000")
            return 5000

    @property
    def downloader_behavior_settings(self) -> dict:
        return self.settings.downloader_behavior

    @property
    def filter_settings(self) -> dict:
        return self.settings.filter

    def update_local_path(self, path: str) -> None:
        """
        Update the local storage path.

        Args:
            path: New local storage path
        """
        # Note: This updates the runtime settings, not the persistent configuration
        self.settings.storage["PATH"] = path
        logger.info(f"Updated local storage path to: {path}")

    def update_settings(self, category: str, settings_to_update: dict) -> None:
        """
        Update settings for a specific category.

        Args:
            category: The category of settings to update (e.g., 's3', 'storage').
            settings_to_update: Dictionary with new settings.
        """
        target_dict = getattr(self.settings, category, None)
        if target_dict is None or not isinstance(target_dict, dict):
            logger.error(f"Settings category '{category}' not found or is not a dictionary.")
            return

        for key, value in settings_to_update.items():
            if key in target_dict:
                try:
                    original_type = type(target_dict[key])
                    target_dict[key] = original_type(value)
                except (ValueError, TypeError) as e:
                    logger.warning(
                        f"Error converting value for {key}: {e}. "
                        f"Assigning value without type conversion."
                    )
                    target_dict[key] = value
            else:
                logger.warning(
                    f"Attempted to update non-existent setting '{key}' in category '{category}'"
                )

        logger.info(f"Updated settings for category '{category}': {settings_to_update}")
        self._log_current_settings()
