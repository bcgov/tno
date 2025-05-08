"""
S3 Client module for downloading files from S3 bucket.
"""
import os
import logging
import socket  
from pathlib import Path
from typing import List, Optional

import boto3
import botocore.config
from botocore.exceptions import ClientError

# type hint for TimeoutError
TimeoutError = socket.timeout


logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
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
        timeout: int = 2  
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
            connect_timeout=timeout,
            read_timeout=timeout,
            retries={'max_attempts': 1}  
        )

        # initialize S3 client
        self.s3_client = boto3.client(
            's3',
            endpoint_url=endpoint_url,
            aws_access_key_id=access_key,
            aws_secret_access_key=secret_key,
            config=config
        )

        logger.info(f"S3 client initialized for bucket: {bucket_name}")

    def list_objects(self, prefix: str = "") -> List[dict]:
        """
        List objects in the S3 bucket with the given prefix.

        Args:
            prefix: Prefix to filter objects

        Returns:
            List of objects in the bucket
        """
        try:
            response = self.s3_client.list_objects_v2(
                Bucket=self.bucket_name,
                Prefix=prefix
            )

            if 'Contents' in response:
                logger.info(f"Found {len(response['Contents'])} objects with prefix '{prefix}'")
                return response['Contents']
            else:
                logger.info(f"No objects found with prefix '{prefix}'")
                return []

        except ClientError as e:
            logger.error(f"Error listing objects: {e}")
            return []

    def download_file(self, s3_key: str, local_path: Optional[str] = None) -> bool:
        """
        Download a file from S3.

        Args:
            s3_key: S3 object key
            local_path: Local path to save the file. If None, will use the key name
                        in the local_storage_path

        Returns:
            True if download was successful, False otherwise
        """
        if local_path is None:
            # use the key name as the local file name
            file_name = os.path.basename(s3_key)
            local_path = self.local_storage_path / file_name
        else:
            local_path = Path(local_path)

        # ensure parent directory exists
        local_path.parent.mkdir(parents=True, exist_ok=True)

        try:
            logger.info(f"Downloading {s3_key} to {local_path}")
            self.s3_client.download_file(
                self.bucket_name,
                s3_key,
                str(local_path)
            )
            logger.info(f"Successfully downloaded {s3_key}")
            return True
        except ClientError as e:
            logger.error(f"Error downloading {s3_key}: {e}")
            return False

    def download_directory(self, prefix: str, local_dir: Optional[str] = None) -> int:
        """
        Download all files in a directory (prefix) from S3.

        Args:
            prefix: S3 prefix (directory)
            local_dir: Local directory to save files. If None, will use
                      local_storage_path/prefix

        Returns:
            Number of files successfully downloaded
        """
        if local_dir is None:
            local_dir = self.local_storage_path / prefix
        else:
            local_dir = Path(local_dir)

        # ensure directory exists
        local_dir.mkdir(parents=True, exist_ok=True)

        objects = self.list_objects(prefix)
        successful_downloads = 0

        for obj in objects:
            # skip directories (objects ending with '/')
            if obj['Key'].endswith('/'):
                continue

            # calculate relative path
            rel_path = os.path.relpath(obj['Key'], prefix)
            local_path = local_dir / rel_path

            if self.download_file(obj['Key'], local_path):
                successful_downloads += 1

        logger.info(f"Downloaded {successful_downloads} files from {prefix}")
        return successful_downloads

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
            error_code = e.response.get('Error', {}).get('Code', 'Unknown')
            logger.error(f"S3 client error (code: {error_code}): {e}")
            return False
        except TimeoutError as e:
            logger.error(f"Connection timeout: {e}")
            return False
        except Exception as e:
            logger.error(f"Unexpected error testing connection: {type(e).__name__}: {e}")
            return False
