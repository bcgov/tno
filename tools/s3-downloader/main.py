"""
S3 Downloader main script.
"""
import os
from pathlib import Path
from dotenv import load_dotenv

from src.client.s3_client import S3Client

load_dotenv()

def main():
    """Main function to download files from S3."""
    print("Starting S3 downloader...")

    bucket_name = os.getenv("S3_BUCKET_NAME", "local")
    endpoint_url = os.getenv("S3_SERVICE_URL", "https://gcpe-mmi-storage.objectstore.gov.bc.ca")
    access_key = os.getenv("S3_ACCESS_KEY", "your-access-key")
    secret_key = os.getenv("S3_SECRET_KEY", "your-secret-key")
    local_storage_path = os.getenv("LOCAL_STORAGE_PATH", "./downloads")
    timeout = int(os.getenv("S3_TIMEOUT", "2"))  

    target_directory = "TC"  


    s3_client = S3Client(
        bucket_name=bucket_name,
        endpoint_url=endpoint_url,
        access_key=access_key,
        secret_key=secret_key,
        local_storage_path=local_storage_path,
        timeout=timeout  
    )
    if not s3_client.test_connection():
        print(f"Connection to {bucket_name} failed. Exiting...")
        return

    objects = s3_client.list_objects(prefix=target_directory)

    if objects:
        print(f"Downloading directory {target_directory}...")
        downloaded = s3_client.download_directory(target_directory)
        print(f"Successfully downloaded {downloaded} files")


if __name__ == "__main__":
    main()
