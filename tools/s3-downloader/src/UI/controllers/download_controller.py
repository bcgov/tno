import logging
from pathlib import Path

from PySide6.QtCore import QObject, Signal

from ..s3_controller import S3Controller

logger = logging.getLogger(__name__)


class DownloadController(QObject):
    """Controller for download operations."""

    # Signals
    download_started = Signal()
    download_stopped = Signal()
    download_progress = Signal(int, str)  # progress, message
    download_finished = Signal(bool, str, object)  # success, message, data
    connection_test_started = Signal()
    connection_test_finished = Signal(bool, str)  # success, message

    def __init__(self, settings_controller):
        """
        Initialize the download controller.

        Args:
            settings_controller: Settings controller instance
        """
        super().__init__()
        self.settings = settings_controller
        self.s3_controller = S3Controller()
        self.is_downloading = False

    def test_connection(self):
        """Test connection to S3."""
        # Get connection parameters from settings
        endpoint_url = self.settings.endpoint_url
        bucket_name = self.settings.bucket_name
        access_key = self.settings.access_key
        secret_key = self.settings.secret_key
        timeout = self.settings.timeout
        local_path = self.settings.local_path

        # Validate required fields
        if not all([endpoint_url, bucket_name, access_key, secret_key]):
            error_msg = "Error: Please set S3 connection parameters in .env file"
            logger.error(error_msg)
            self.connection_test_finished.emit(False, error_msg)
            return

        logger.info(f"Testing connection to {bucket_name} with {timeout}s timeout...")
        self.connection_test_started.emit()

        # Initialize S3 client
        if not self.s3_controller.initialize_client(
            bucket_name=bucket_name,
            endpoint_url=endpoint_url,
            access_key=access_key,
            secret_key=secret_key,
            local_storage_path=local_path,
            timeout=timeout,
        ):
            error_msg = "Error: Failed to initialize S3 client"
            logger.error(error_msg)
            self.connection_test_finished.emit(False, error_msg)
            return

        # Test connection
        self.s3_controller.test_connection(self._on_connection_test_finished)

    def toggle_download(self):
        """Toggle download state."""
        if self.is_downloading:
            self._stop_download()
        else:
            self._start_download()

    def _start_download(self):
        """Start the download process."""
        # Before starting, ensure S3 client is initialized and path is set
        if not hasattr(self.s3_controller, "s3_client") or self.s3_controller.s3_client is None:
            error_msg = "Error: S3 client not initialized. Please test connection first."
            logger.error(error_msg)
            self.download_finished.emit(False, error_msg, None)
            return

        storage_path = self.settings.local_path
        if not storage_path:
            error_msg = "Error: Storage path not set. Please select a storage path."
            logger.error(error_msg)
            self.download_finished.emit(False, error_msg, None)
            return

        # Ensure storage path exists
        Path(storage_path).mkdir(parents=True, exist_ok=True)

        # Set downloading state
        self.is_downloading = True
        self.download_started.emit()

        # Execute download task
        self.execute_download_task()

    def _stop_download(self):
        """Stop the download process."""
        self.is_downloading = False
        self.download_stopped.emit()
        logger.info("Download stopped")

    def execute_download_task(self):
        """Execute the download task."""
        if not self.is_downloading:
            return

        storage_path = self.settings.local_path
        logger.info(f"Executing download task to {storage_path}")

        # Call S3 controller to download files with network error handling parameters
        self.s3_controller.download_files(
            prefix="",  # Download all files
            local_dir=storage_path,
            on_progress=self._on_download_progress,
            on_finished=self._on_download_finished,
            max_consecutive_failures=self.settings.max_consecutive_failures,
            max_failure_percentage=self.settings.max_failure_percentage,
            network_test_interval=self.settings.network_test_interval,
        )

    def _on_connection_test_finished(self, success, message, data=None):
        """
        Handle connection test result.

        Args:
            success: Whether the test was successful
            message: Result message
            data: Additional data (if any)
        """
        if success:
            logger.info("Connection test successful")
        else:
            logger.error(f"Connection test failed: {message}")

        self.connection_test_finished.emit(success, message)

    def _on_download_progress(self, progress, message):
        """
        Handle download progress updates.

        Args:
            progress: Progress percentage (0-100)
            message: Progress message
        """
        self.download_progress.emit(progress, message)

    def _on_download_finished(self, success, message, data=None):
        """
        Handle download completion.

        Args:
            success: Whether the download was successful
            message: Result message
            data: Additional data (if any)
        """
        logger.info(f"Download finished: {message}")
        self.download_finished.emit(success, message, data)
