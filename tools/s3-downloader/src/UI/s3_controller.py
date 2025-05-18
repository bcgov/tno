"""
Controller for connecting the S3 client with the UI.
"""

import logging
from pathlib import Path
from typing import Callable

from PySide6.QtCore import QObject, QThread, Signal

from ..client.s3_client import S3Client

logging.basicConfig(
    level=logging.INFO, format="%(asctime)s - %(name)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger(__name__)


class S3Worker(QThread):
    """Worker thread for S3 operations."""

    # signal definitions
    finished = Signal(bool, str, object)  # success/failure, message, optional data
    progress = Signal(int, str)  # progress percentage, message

    def __init__(self, operation: str, s3_client: S3Client, **kwargs):
        """
        Initialize the worker.

        Args:
            operation: Operation to perform ('test', 'list', 'download')
            s3_client: S3 client instance
            **kwargs: Additional arguments for the operation
        """
        super().__init__()
        self.operation = operation
        self.s3_client = s3_client
        self.kwargs = kwargs

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

                # get object list
                files = self.s3_client.list_objects(prefix=prefix)
                if not files:
                    self.finished.emit(True, "No files to download", None)
                    return

                # download files
                total_files = len(files)
                successful = 0
                failed = 0

                for i, obj in enumerate(files):
                    # update progress
                    progress = int((i + 1) / total_files * 100)
                    self.progress.emit(progress, f"Downloading {obj['Key']}...")

                    # calculate local path, keeping original directory structure
                    key_parts = obj["Key"].split("/")
                    local_path = bucket_dir

                    # create directory structure
                    for part in key_parts[:-1]:
                        if part:  # skip empty parts
                            local_path = local_path / part
                            local_path.mkdir(exist_ok=True)

                    # add file name
                    if key_parts[-1]:  # ensure not a directory
                        local_path = local_path / key_parts[-1]

                        # download file
                        if self.s3_client.download_file(obj["Key"], local_path):
                            successful += 1
                        else:
                            failed += 1
                            logger.error(f"Failed to download: {obj['Key']}")

                # build result data
                result = {"total": total_files, "successful": successful, "failed": failed}

                # build detailed message
                message = f"Downloaded {successful} of {total_files} files to {bucket_dir}"
                if failed > 0:
                    message += f" ({failed} files failed)"

                self.finished.emit(True, message, result)

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
    ):
        """
        Download files from S3.

        Args:
            prefix: Object prefix
            local_dir: Local directory to save files
            on_progress: Callback for progress updates
            on_finished: Callback for when the operation finishes
        """
        if not self.s3_client:
            on_finished(False, "S3 client not initialized")
            return

        self.worker = S3Worker("download", self.s3_client, prefix=prefix, local_dir=local_dir)
        self.worker.progress.connect(on_progress)
        self.worker.finished.connect(on_finished)
        self.worker.start()
