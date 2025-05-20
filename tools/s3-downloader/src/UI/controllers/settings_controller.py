import logging

logger = logging.getLogger(__name__)


class SettingsController:
    """Controller for application settings."""

    def __init__(self):
        """Initialize the settings controller."""
        self.endpoint_url = ""
        self.bucket_name = ""
        self.access_key = ""
        self.secret_key = ""
        self.timeout = 2
        self.local_path = ""
        self.scheduler_interval = 3600
        self.max_consecutive_failures = 3
        self.max_failure_percentage = 0.3
        self.network_test_interval = 5
        self.space_warning_threshold = 0.1

        # Load settings
        self.load_settings()

    def load_settings(self):
        """Load settings from settings module."""
        try:
            from src.settings import settings

            # S3 connection settings
            self.endpoint_url = settings.s3["SERVICE_URL"]
            self.bucket_name = settings.s3["BUCKET_NAME"]
            self.access_key = settings.s3["ACCESS_KEY"]
            self.secret_key = settings.s3["SECRET_KEY"]
            self.timeout = settings.s3["TIMEOUT"]

            # Local storage settings
            self.local_path = settings.storage["PATH"]

            # Scheduler settings
            self.scheduler_interval = settings.scheduler["INTERVAL"]

            # Network error handling settings
            self.max_consecutive_failures = settings.error_handling["MAX_CONSECUTIVE_FAILURES"]
            self.max_failure_percentage = settings.error_handling["MAX_FAILURE_PERCENTAGE"]
            self.network_test_interval = settings.error_handling["NETWORK_TEST_INTERVAL"]

            # Disk space settings
            self.space_warning_threshold = settings.disk_space["WARNING_THRESHOLD"]

            # Log loaded settings
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
                f"space_warning_threshold={self.space_warning_threshold}"
            )
        except Exception as e:
            logger.error(f"Error loading settings: {e}")

    def update_local_path(self, path):
        """
        Update the local storage path.

        Args:
            path: New local storage path
        """
        self.local_path = path
