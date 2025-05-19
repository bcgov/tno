"""
Default settings for S3 downloader.
"""

# S3 Configuration
S3_SETTINGS = {
    # S3 Service URL
    "SERVICE_URL": "url",
    # S3 Bucket Name
    "BUCKET_NAME": "local",
    # S3 Access Key
    "ACCESS_KEY": "",
    # S3 Secret Key
    "SECRET_KEY": "",
    # Connection Timeout (seconds)
    "TIMEOUT": 2,
}

# Local Storage Configuration
STORAGE_SETTINGS = {
    # Local Storage Path
    "PATH": "./downloads",
}

# Database Configuration
DATABASE_SETTINGS = {
    # Database URL
    "URL": "sqlite:///./s3_downloader.db",
}

# Scheduler Configuration
SCHEDULER_SETTINGS = {
    # Download Interval (seconds)
    "INTERVAL": 3600,
}

# Network Error Handling Configuration
ERROR_HANDLING_SETTINGS = {
    # Maximum Consecutive Failures
    "MAX_CONSECUTIVE_FAILURES": 3,
    # Maximum Failure Percentage (0.0-1.0)
    "MAX_FAILURE_PERCENTAGE": 0.3,
    # Network Test Interval (failure count)
    "NETWORK_TEST_INTERVAL": 5,
}

# Disk Space Configuration
DISK_SPACE_SETTINGS = {
    # Minimum Available Disk Space Ratio (0.0-1.0)
    "WARNING_THRESHOLD": 0.1,
}
