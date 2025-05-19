import logging
import os
import socket
from pathlib import Path
from typing import Any, Callable, Dict, List, Optional, Tuple

import boto3
import botocore.config
from botocore.exceptions import (
    ClientError,
    ConnectionClosedError,
    ConnectTimeoutError,
    ReadTimeoutError,
)

# Import database models
from ..database.models import DownloadedFile, DownloadTask
from ..settings import settings
from ..settings.default_settings import DOWNLOADER_BEHAVIOR_SETTINGS

# type hint for TimeoutError
TimeoutError = socket.timeout


logging.basicConfig(
    level=logging.INFO, format="%(asctime)s - %(name)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger(__name__)


class S3Client:
    """S3 Client for downloading files from S3 bucket."""

    def __init__(
        self,
        bucket_name: str,
        endpoint_url: str,
        access_key: str,
        secret_key: str,
        local_storage_path: str,
        timeout: int = 2,
    ):
        """
        Initialize S3 client.

        Args:
            bucket_name: Name of the S3 bucket
            endpoint_url: S3 service URL
            access_key: S3 access key
            secret_key: S3 secret key
            local_storage_path: Path to store downloaded files
            timeout: Connection timeout in seconds (default: 2)
        """
        self.bucket_name = bucket_name
        self.local_storage_path = Path(local_storage_path)
        self.timeout = timeout

        # ensure local storage path exists
        self.local_storage_path.mkdir(parents=True, exist_ok=True)

        # define botocore config with timeout settings
        config = botocore.config.Config(
            connect_timeout=timeout, read_timeout=timeout, retries={"max_attempts": 1}
        )

        # initialize S3 client
        self.s3_client = boto3.client(
            "s3",
            endpoint_url=endpoint_url,
            aws_access_key_id=access_key,
            aws_secret_access_key=secret_key,
            config=config,
        )

        logger.info(f"S3 client initialized for bucket: {bucket_name}")

    def list_objects(
        self,
        prefix: str = "",
        include_directories: bool = False,
        exclude_downloaded: bool = False,
        exclude_prefixes: Optional[List[str]] = None,
    ) -> List[dict]:
        """
        List objects in the S3 bucket with the given prefix.

        Args:
            prefix: Prefix to filter objects
            include_directories: Whether to include directories (objects ending with '/')
            exclude_downloaded: Whether to exclude already downloaded files from the result
            exclude_prefixes: List of prefixes/directories to exclude (e.g., ['bb/', 'temp/'])
                            Defaults to FILTER_SETTINGS["EXCLUDE_PREFIXES"]
                            Can be overridden via EXCLUDE_PREFIXES environment variable

        Returns:
            List of objects in the bucket

        Raises:
            ClientError: If there's an error with the S3 service
            ConnectionClosedError, ConnectTimeoutError, ReadTimeoutError, TimeoutError:
                If there's a network-related error
        """
        try:
            # Initialize variables for pagination
            all_objects = []
            continuation_token = None

            # Loop until all objects are fetched
            while True:
                # Prepare request parameters
                params = {"Bucket": self.bucket_name, "Prefix": prefix}

                # Add continuation token if we have one
                if continuation_token:
                    params["ContinuationToken"] = continuation_token

                # Make the request
                response = self.s3_client.list_objects_v2(**params)

                # Add objects to our collection
                if "Contents" in response:
                    all_objects.extend(response["Contents"])
                    logger.debug(f"Fetched batch of {len(response['Contents'])} objects")

                # Check if there are more objects to fetch
                if not response.get("IsTruncated"):  # No more objects
                    break

                # Get the continuation token for the next request
                continuation_token = response.get("NextContinuationToken")

            logger.info(f"Total objects found with prefix '{prefix}': {len(all_objects)}")

            # If no objects found, return empty list
            if not all_objects:
                logger.debug(f"No objects found with prefix '{prefix}'")
                return []

            # If not including directories, filter them out
            if not include_directories:
                filtered_objects = [obj for obj in all_objects if not obj["Key"].endswith("/")]
            else:
                filtered_objects = all_objects

            # If exclude_downloaded is True, filter out already downloaded files
            if exclude_downloaded:
                from ..database.models import DownloadedFile, FileStatus

                # Process in batches to avoid SQL variable limit
                batch_size = DOWNLOADER_BEHAVIOR_SETTINGS["SQL_QUERY_BATCH_SIZE"]
                object_keys = [obj["Key"] for obj in filtered_objects]
                downloaded_keys = set()

                # Process keys in batches
                for i in range(0, len(object_keys), batch_size):
                    batch = object_keys[i : i + batch_size]
                    batch_downloaded = set(
                        file.s3_key
                        for file in DownloadedFile.select(DownloadedFile.s3_key).where(
                            (DownloadedFile.status == FileStatus.COMPLETED.value)
                            & (DownloadedFile.s3_key.in_(batch))  # type: ignore
                        )
                    )
                    downloaded_keys.update(batch_downloaded)

                # Filter out already downloaded files
                original_count = len(filtered_objects)
                filtered_objects = [
                    obj for obj in filtered_objects if obj["Key"] not in downloaded_keys
                ]
                skipped_count = original_count - len(filtered_objects)
                logger.info(f"Filtered out {skipped_count} already downloaded files")

            if not include_directories:
                logger.debug(
                    f"Found {len(filtered_objects)} files with prefix '{prefix}' (excluding directories{', excluding downloaded' if exclude_downloaded else ''})"
                )
            else:
                logger.debug(
                    f"Found {len(filtered_objects)} objects with prefix '{prefix}'{' (excluding downloaded)' if exclude_downloaded else ''}"
                )

            # If exclude_prefixes is provided, filter out objects that match any of the prefixes
            if exclude_prefixes:
                filtered_objects = [
                    obj
                    for obj in filtered_objects
                    if not any(
                        obj["Key"].startswith(exclude_prefix) for exclude_prefix in exclude_prefixes
                    )
                ]

            return filtered_objects

        except (ConnectionClosedError, ConnectTimeoutError, ReadTimeoutError, TimeoutError) as e:
            logger.error(f"Network error listing objects: {str(e)}")
            # Re-raise network errors to be handled by the caller
            raise

        except ClientError as e:
            logger.error(f"S3 error listing objects: {e}")
            # Re-raise client errors to be handled by the caller
            raise

        except Exception as e:
            logger.error(f"Unexpected error listing objects: {str(e)}")
            # Re-raise to be handled by the caller
            raise

    def download_file(
        self,
        s3_key: str,
        local_path: Optional[str] = None,
        task_record: Optional[DownloadTask] = None,
    ) -> Tuple[bool, Optional[DownloadedFile]]:
        """
        Download a file from S3.

        Args:
            s3_key: S3 object key
            local_path: Local path to save the file. If None, will use the key name
                        in the local_storage_path
            task_record: Optional DownloadTask record for logging

        Returns:
            Tuple of (success, file_record)
            - success: True if download was successful, False otherwise
            - file_record: DownloadedFile record if task_record was provided, None otherwise

        Raises:
            Various exceptions for network-related errors that should be handled by the caller
        """
        if local_path is None:
            # use the key name as the local file name
            file_name = os.path.basename(s3_key)
            local_path_obj = self.local_storage_path / file_name
        else:
            local_path_obj = Path(local_path)

        # ensure parent directory exists
        local_path_obj.parent.mkdir(parents=True, exist_ok=True)

        # Create file record if task record is provided
        file_record = None
        if task_record:
            file_record = DownloadedFile.create_file(
                task=task_record, s3_key=s3_key, local_path=str(local_path_obj)
            )

        try:
            logger.info(f"Downloading {s3_key} to {local_path_obj}")
            self.s3_client.download_file(self.bucket_name, s3_key, str(local_path_obj))

            # Update file record if it exists
            if file_record:
                file_size = local_path_obj.stat().st_size
                file_record.complete(file_size)

            logger.info(f"Successfully downloaded {s3_key}")
            return True, file_record

        except (ConnectionClosedError, ConnectTimeoutError, ReadTimeoutError, TimeoutError) as e:
            # Network-related errors
            error_msg = f"Network error downloading {s3_key}: {str(e)}"
            logger.error(error_msg)

            # Update file record if it exists
            if file_record:
                file_record.fail(error_msg)

            # Re-raise the exception to allow the caller to handle network errors
            raise

        except ClientError as e:
            # S3-specific errors
            error_msg = f"S3 error downloading {s3_key}: {e}"
            logger.error(error_msg)

            # Update file record if it exists
            if file_record:
                file_record.fail(error_msg)

            return False, file_record

        except Exception as e:
            # Other unexpected errors
            error_msg = f"Unexpected error downloading {s3_key}: {str(e)}"
            logger.error(error_msg)

            # Update file record if it exists
            if file_record:
                file_record.fail(error_msg)

            return False, file_record

    def download_directory(
        self,
        prefix: str,
        local_dir: Optional[str] = None,
        max_consecutive_failures: int = 3,
        max_failure_percentage: float = 0.3,
        network_test_interval: int = 5,
        on_progress: Optional[Callable[[int, str], None]] = None,
        exclude_downloaded: bool = True,
        max_files_per_task: Optional[int] = DOWNLOADER_BEHAVIOR_SETTINGS["MAX_FILES_PER_TASK"],
        is_cancelled: Optional[Callable[[], bool]] = None,
        exclude_prefixes: Optional[List[str]] = settings.filter["EXCLUDE_PREFIXES"],
    ) -> Dict[str, Any]:
        """
        Download all files in a directory (prefix) from S3.

        Args:
            prefix: S3 prefix (directory)
            local_dir: Local directory to save files. If None, will use
                      local_storage_path/prefix
            max_consecutive_failures: Maximum number of consecutive failures before aborting
            max_failure_percentage: Maximum percentage of failures before aborting (0.0-1.0)
            network_test_interval: Number of failures before testing network connection
            on_progress: Optional callback for progress updates (progress_percentage, message)
            exclude_downloaded: Whether to exclude already downloaded files from the download
            max_files_per_task: Maximum number of files to download per task
            is_cancelled: Optional function that returns True if the download should be cancelled
            exclude_prefixes: List of prefixes/directories to exclude (e.g., ['bb/', 'temp/'])
                            Defaults to FILTER_SETTINGS["EXCLUDE_PREFIXES"]
                            Can be overridden via EXCLUDE_PREFIXES environment variable

        Returns:
            Dictionary with download statistics and task record
        """
        if local_dir is None:
            local_dir_obj = self.local_storage_path / prefix
        else:
            local_dir_obj = Path(local_dir)

        # ensure directory exists
        local_dir_obj.mkdir(parents=True, exist_ok=True)

        # get file list
        try:
            # List objects with exclude_downloaded filter
            objects = self.list_objects(
                prefix, exclude_downloaded=exclude_downloaded, exclude_prefixes=exclude_prefixes
            )

            # If no objects found at all
            if not objects:
                logger.info(f"No files found with prefix '{prefix}' in S3")
                return {
                    "successful": 0,
                    "failed": 0,
                    "total": 0,
                    "skipped": True,
                    "message": "No new files to download",
                    "task_record": None,
                }

            # Apply max_files_per_task limit if specified
            if max_files_per_task is not None and len(objects) > max_files_per_task:
                logger.info(
                    f"Limiting to {max_files_per_task} new files (out of {len(objects)} available)"
                )
                objects = objects[:max_files_per_task]

            # If no new files to download after filtering
            if not objects:
                # All files are already downloaded or no new files to download
                file_count = len(self.list_objects(prefix, exclude_downloaded=False))
                logger.info(f"No new files to download. Total files in S3: {file_count}")
                return {
                    "successful": 0,
                    "failed": 0,
                    "total": 0,
                    "skipped": True,
                    "message": f"No new files to download. Total files in S3: {file_count}",
                    "task_record": None,
                }
        except (
            ClientError,
            ConnectionClosedError,
            ConnectTimeoutError,
            ReadTimeoutError,
            TimeoutError,
        ) as e:
            # Network error during listing objects
            task_record = DownloadTask.create_task(
                s3_prefix=prefix,
                local_path=str(local_dir_obj),
                total_files=0,
                status="Failed",
            )
            error_message = f"Network error while listing objects: {str(e)}"
            task_record.fail(error_message)
            logger.error(error_message)
            return {
                "successful": 0,
                "failed": 0,
                "total": 0,
                "task_record": task_record,
                "error": error_message,
            }

        # Create download task record
        task_record = DownloadTask.create_task(
            s3_prefix=prefix,
            local_path=str(local_dir_obj),
            total_files=len(objects),
            status="In Progress",
        )

        successful_downloads = 0
        failed_downloads = 0
        error_messages = []
        consecutive_failures = 0
        failure_count = 0
        task_aborted = False
        abort_reason = None

        # Start the download process
        total_files = len(objects)
        task_cancelled = False

        for i, obj in enumerate(objects):
            # Check if download has been cancelled
            if is_cancelled and is_cancelled():
                task_cancelled = True
                logger.info("Download cancelled by user")
                break

            # Update progress
            if on_progress and total_files > 0:
                progress_percent = int((i / total_files) * 100)
                on_progress(progress_percent, f"Downloading: {obj['Key']}")

            # Check if we've exceeded the maximum consecutive failures
            if consecutive_failures >= max_consecutive_failures:
                task_aborted = True
                abort_reason = f"Aborted after {consecutive_failures} consecutive download failures"
                logger.error(abort_reason)
                break

            # Check if we've exceeded the maximum failure percentage
            if i > 0 and (failure_count / i) > max_failure_percentage:
                task_aborted = True
                abort_reason = f"Aborted after failure rate exceeded {max_failure_percentage * 100}% ({failure_count}/{i})"
                logger.error(abort_reason)
                break

            # Test network connection after multiple failures
            if consecutive_failures > 0 and consecutive_failures % network_test_interval == 0:
                logger.info(
                    f"Testing network connection after {consecutive_failures} consecutive failures"
                )
                if not self.test_connection():
                    task_aborted = True
                    abort_reason = "Network connection test failed, aborting download task"
                    logger.error(abort_reason)
                    break

            # calculate relative path
            rel_path = os.path.relpath(obj["Key"], prefix)
            local_path = str(local_dir_obj / rel_path)

            try:
                success, file_record = self.download_file(obj["Key"], local_path, task_record)
                if success:
                    successful_downloads += 1
                    consecutive_failures = 0  # Reset consecutive failures counter
                else:
                    failed_downloads += 1
                    failure_count += 1
                    consecutive_failures += 1
                    # Collect error message if available
                    if file_record and file_record.error_message:
                        error_messages.append(file_record.error_message)
            except Exception as e:
                # Handle unexpected exceptions during download
                failed_downloads += 1
                failure_count += 1
                consecutive_failures += 1
                error_msg = f"Unexpected error downloading {obj['Key']}: {str(e)}"
                logger.error(error_msg)
                error_messages.append(error_msg)

                # Create a failed file record if it doesn't exist
                if task_record:
                    DownloadedFile.create_file(
                        task=task_record,
                        s3_key=obj["Key"],
                        local_path=local_path,
                        status="Failed",
                        error_message=error_msg,
                    )

            # Update progress after each file
            if on_progress and total_files > 0:
                progress_percent = int(((i + 1) / total_files) * 100)
                on_progress(progress_percent, f"Downloaded: {obj['Key']}")

        # Generate error summary
        error_summary = None
        if failed_downloads > 0 or task_aborted:
            # Count occurrences of each error message
            error_counts = {}
            for error in error_messages:
                if error in error_counts:
                    error_counts[error] += 1
                else:
                    error_counts[error] = 1

            # Format error summary
            if task_aborted and abort_reason:
                error_summary = f"Download task aborted: {abort_reason}\n\n"
            else:
                error_summary = f"Failed to download {failed_downloads} files.\n\n"

            if error_counts:
                error_summary += "Error summary:\n"
                for error, count in error_counts.items():
                    error_summary += f"- {error} ({count} files)\n"
            else:
                error_summary += "No specific error messages recorded."

        # Update task record
        if task_cancelled or task_aborted:
            # Task was stopped (either by user or due to errors)
            abort_message = "Download stopped"
            if task_cancelled:
                abort_message = "Download cancelled by user"
            elif abort_reason:
                abort_message = abort_reason

            # Pass successful and failed download counts to the abort method
            task_record.abort(abort_message, successful_downloads, failed_downloads)
            logger.warning(
                f"Download task aborted: {len(objects) - (successful_downloads + failed_downloads)} files were not processed"
            )
        else:
            # Task completed normally
            task_record.complete(successful_downloads, failed_downloads, error_summary)
            logger.info(
                f"Downloaded {successful_downloads} files from {prefix} ({failed_downloads} failed)"
            )

        if error_summary:
            logger.error(error_summary)

        # Send final progress update
        if on_progress and len(objects) > 0:
            on_progress(100, f"Download complete. {successful_downloads} files downloaded.")

        # Return statistics and task record
        result = {
            "successful": successful_downloads,
            "failed": failed_downloads,
            "total": len(objects),
            "task_record": task_record,
        }

        if task_aborted or task_cancelled:
            result["aborted"] = True
            if task_cancelled:
                result["abort_reason"] = "Download cancelled by user"
            elif abort_reason:
                result["abort_reason"] = abort_reason

        return result

    def test_connection(self) -> bool:
        """
        Test the network connection to S3.

        Returns:
            True if connection is successful, False otherwise
        """
        logger.info(f"Testing connection to {self.bucket_name} with {self.timeout}s timeout...")
        try:
            self.s3_client.head_bucket(Bucket=self.bucket_name)
            logger.info("Connection successful!")
            return True
        except ClientError as e:
            error_code = e.response.get("Error", {}).get("Code", "Unknown")
            logger.error(f"S3 client error (code: {error_code}): {e}")
            return False
        except TimeoutError as e:
            logger.error(f"Connection timeout: {e}")
            return False
        except Exception as e:
            logger.error(f"Unexpected error testing connection: {type(e).__name__}: {e}")
            return False
