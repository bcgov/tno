"""
Settings module for S3 downloader.
"""

from .settings_loader import settings

# Load settings from environment variables
settings.load_from_env()

# Export settings instance
__all__ = ["settings"]
