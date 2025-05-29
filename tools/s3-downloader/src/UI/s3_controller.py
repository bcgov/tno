"""
Controller for connecting the S3 client with the UI.
"""

import logging
from pathlib import Path
from typing import Any, Callable, Dict, List

from PySide6.QtCore import QObject, QThread, Signal

from ..client.s3_client import S3Client
from ..database.models import DownloadedFile, DownloadTask
from ..settings.settings_loader import settings

logging.basicConfig(
    level=logging.INFO, format="%(asctime)s - %(name)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger(__name__)


class S3Worker(QThread):
    """Worker thread for S3 operations."""

    # signal definitions
    finished = Signal(bool, str, object)  # success/failure, message, optional data
    progress = Signal(int, str)  # progress percentage, message
    cancelled = Signal()  # Signal emitted when download is cancelled

    def __init__(self, operation: str, s3_client: S3Client, **kwargs):
        """
        Initialize the worker thread.

        Args:
            operation: Operation to perform (e.g., test, list, download)
            s3_client: S3 client instance
            **kwargs: Additional arguments for the operation
        """
        super().__init__()
        self.operation = operation
        self.s3_client = s3_client
        self.kwargs = kwargs
        self._is_cancelled = False  # Flag to track cancellation status

        # Network error handling parameters with defaults
        self.max_consecutive_failures = kwargs.get("max_consecutive_failures", 3)
        self.max_failure_percentage = kwargs.get("max_failure_percentage", 0.3)
        self.network_test_interval = kwargs.get("network_test_interval", 5)

    def cancel(self):
        """Request cancellation of the current operation."""
        if self.operation == "download" and self.isRunning():
            logger.info("Cancelling download operation...")
            self._is_cancelled = True
            self.cancelled.emit()
        else:
            logger.warning(f"Cannot cancel operation '{self.operation}' or thread not running")

    def run(self):
        """Run the worker thread."""
        try:
            if self.operation == "test":
                success = self.s3_client.test_connection()
                if success:
                    self.finished.emit(True, "Connection successful", None)
                else:
                    self.finished.emit(False, "Connection failed", None)

            elif self.operation == "list":
                prefix = self.kwargs.get("prefix", "")
                # get file list
                objects = self.s3_client.list_objects(prefix=prefix)

                if objects:
                    # build details
                    result = {
                        "total": len(objects),
                        "objects": objects,  # pass full object list
                    }

                    message = f"Found {len(objects)} files"
                    self.finished.emit(True, message, result)
                else:
                    self.finished.emit(True, "No files found", {"total": 0, "objects": []})

            elif self.operation == "download":
                prefix = self.kwargs.get("prefix", "")
                local_dir = self.kwargs.get("local_dir")

                if not local_dir:
                    self.finished.emit(False, "Local directory not specified", None)
                    return

                # create a subdirectory, named after the bucket
                bucket_dir = Path(local_dir) / self.s3_client.bucket_name
                bucket_dir.mkdir(parents=True, exist_ok=True)

                # use download_directory method to download files, keeping original directory structure
                self.progress.emit(0, f"Starting download to {bucket_dir}...")

                # Download files and record in database
                try:
                    # Get max_files_per_task from settings or use default
                    max_files_per_task = settings.downloader_behavior["MAX_FILES_PER_TASK"]

                    # Use the download_directory method which now creates database records
                    # Pass the network error handling parameters from the worker
                    # Also pass the progress callback
                    download_result = self.s3_client.download_directory(
                        prefix=prefix,
                        local_dir=str(bucket_dir),
                        max_consecutive_failures=self.max_consecutive_failures,
                        max_failure_percentage=self.max_failure_percentage,
                        network_test_interval=self.network_test_interval,
                        on_progress=self.progress.emit,
                        exclude_downloaded=True,
                        max_files_per_task=max_files_per_task,  # Use configuration value
                        is_cancelled=lambda: self._is_cancelled,  # Pass cancellation checker
                    )

                    # check if skipped
                    if download_result.get("skipped", False):
                        # no new files to download
                        message = download_result.get("message", "No files to download")
                        self.progress.emit(100, message)
                        self.finished.emit(
                            True,
                            message,
                            {"total": 0, "successful": 0, "failed": 0, "skipped": True},
                        )
                        return

                    # files already downloaded
                    successful = download_result["successful"]
                    failed = download_result["failed"]
                    total_files = download_result["total"]
                    task_record = download_result["task_record"]

                    # build result data including task record ID for history lookup
                    result = {
                        "total": total_files,
                        "successful": successful,
                        "failed": failed,
                        "task_id": task_record.id,
                    }

                    # Check if the task was aborted (for any reason)
                    if "aborted" in download_result and download_result["aborted"]:
                        # Task was stopped (either by user or due to errors)
                        abort_reason = download_result.get("abort_reason", "Unknown reason")
                        message = f"Download stopped: {abort_reason}"
                        # Add abort information to result
                        result["aborted"] = True
                        result["abort_reason"] = abort_reason
                        # Still return success=True since we handled the abort gracefully
                        self.finished.emit(True, message, result)
                    else:
                        # build detailed message for normal completion
                        message = f"Downloaded {successful} of {total_files} files to {bucket_dir}"
                        if failed > 0:
                            message += f" ({failed} files failed)"
                        self.finished.emit(True, message, result)
                except Exception as e:
                    # other exceptions
                    logger.error(f"Error in download operation: {e}")
                    self.finished.emit(False, f"Error downloading files: {str(e)}", None)

            else:
                self.finished.emit(False, f"Unknown operation: {self.operation}", None)

        except Exception as e:
            logger.error(f"Error in worker thread: {e}")
            self.finished.emit(False, f"Error: {str(e)}", None)


class S3Controller(QObject):
    """Controller for S3 operations."""

    def __init__(self):
        """Initialize the controller."""
        super().__init__()
        self.s3_client = None
        self.worker = None

    def get_download_history(self, limit: int = 10) -> List[DownloadTask]:
        """
        Get download history.

        Args:
            limit: Maximum number of tasks to return

        Returns:
            List of download tasks
        """
        return list(DownloadTask.get_recent_tasks(limit))

    def get_download_details(self, task_id: int) -> Dict[str, Any]:
        """
        Get download details for a specific task.

        Args:
            task_id: Task ID

        Returns:
            Dictionary with task and files information
        """
        task = DownloadTask.get_by_id(task_id)
        files = list(DownloadedFile.get_files_for_task(task_id))

        return {"task": task, "files": files}

    def get_downloaded_files_count(self) -> int:
        """
        Get count of downloaded files.

        Returns:
            Number of downloaded files
        """
        return DownloadedFile.get_total_downloaded_count()

    def initialize_client(
        self,
        bucket_name: str,
        endpoint_url: str,
        access_key: str,
        secret_key: str,
        local_storage_path: str,
        timeout: int = 2,
    ) -> bool:
        """
        Initialize the S3 client.

        Args:
            bucket_name: S3 bucket name
            endpoint_url: S3 endpoint URL
            access_key: S3 access key
            secret_key: S3 secret key
            local_storage_path: Local storage path
            timeout: Connection timeout in seconds

        Returns:
            True if client was initialized successfully
        """
        try:
            self.s3_client = S3Client(
                bucket_name=bucket_name,
                endpoint_url=endpoint_url,
                access_key=access_key,
                secret_key=secret_key,
                local_storage_path=local_storage_path,
                timeout=timeout,
            )
            return True
        except Exception as e:
            logger.error(f"Error initializing S3 client: {e}")
            return False

    def test_connection(self, on_finished: Callable[[bool, str], None]):
        """
        Test connection to S3.

        Args:
            on_finished: Callback for when the operation finishes
        """
        if not self.s3_client:
            on_finished(False, "S3 client not initialized")
            return

        # create and start worker thread
        self.worker = S3Worker("test", self.s3_client)
        self.worker.finished.connect(on_finished)
        self.worker.start()

    def list_objects(self, prefix: str, on_finished: Callable[[bool, str], None]):
        """
        List objects in the S3 bucket.

        Args:
            prefix: Object prefix
            on_finished: Callback for when the operation finishes
        """
        if not self.s3_client:
            on_finished(False, "S3 client not initialized")
            return

        self.worker = S3Worker("list", self.s3_client, prefix=prefix)
        self.worker.finished.connect(on_finished)
        self.worker.start()

    def download_files(
        self,
        prefix: str,
        local_dir: str,
        on_progress: Callable[[int, str], None],
        on_finished: Callable[[bool, str], None],
        max_consecutive_failures: int = 3,
        max_failure_percentage: float = 0.3,
        network_test_interval: int = 5,
    ):
        """
        Download files from S3.

        Args:
            prefix: Object prefix
            local_dir: Local directory to save files
            on_progress: Callback for progress updates
            on_finished: Callback for when the operation finishes
            max_consecutive_failures: Maximum number of consecutive failures before aborting
            max_failure_percentage: Maximum percentage of failures before aborting (0.0-1.0)
            network_test_interval: Number of failures before testing network connection
        """
        if not self.s3_client:
            on_finished(False, "S3 client not initialized")
            return

        self.worker = S3Worker(
            "download",
            self.s3_client,
            prefix=prefix,
            local_dir=local_dir,
            max_consecutive_failures=max_consecutive_failures,
            max_failure_percentage=max_failure_percentage,
            network_test_interval=network_test_interval,
        )
        self.worker.progress.connect(on_progress)
        self.worker.finished.connect(on_finished)
        self.worker.start()

    def cancel_download(self):
        """
        Cancel the current download operation.

        Returns:
            bool: True if cancellation was requested, False if no download is active
        """
        if self.worker and self.worker.operation == "download" and self.worker.isRunning():
            logger.info("Requesting download cancellation")
            self.worker.cancel()
            return True
        else:
            logger.warning("No active download to cancel")
            return False
