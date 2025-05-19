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
                f"endpoint={self.endpoint_url}, "
                f"bucket={self.bucket_name}, "
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

    # Local storage settings
    @property
    def local_path(self) -> str:
        return self.settings.storage["PATH"]

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

    # Disk space settings
    @property
    def space_warning_threshold(self) -> float:
        return self.settings.disk_space["WARNING_THRESHOLD"]

    # Downloader behavior settings
    @property
    def max_files_per_task(self) -> int:
        try:
            return self.settings.downloader_behavior["MAX_FILES_PER_TASK"]
        except (AttributeError, KeyError):
            logger.warning("MAX_FILES_PER_TASK not found in settings, using default value 5000")
            return 5000

    def update_local_path(self, path: str) -> None:
        """
        Update the local storage path.

        Args:
            path: New local storage path
        """
        # Note: This updates the runtime settings, not the persistent configuration
        self.settings.storage["PATH"] = path
        logger.info(f"Updated local storage path to: {path}")
