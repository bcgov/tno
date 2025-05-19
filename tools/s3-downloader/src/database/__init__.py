"""
Database initialization for S3 downloader.
"""

import logging

from peewee import SqliteDatabase

from .models import DownloadedFile, DownloadTask, db

logger = logging.getLogger(__name__)


def get_db_path() -> str:
    """
    Get database path from settings.

    Returns:
        Database path
    """
    from src.settings import settings

    db_path = settings.database["URL"]

    # If the path is a SQLite URL, extract the file path
    if db_path.startswith("sqlite:///"):
        db_path = db_path[10:]  # Remove 'sqlite:///'

    return db_path


def initialize_database() -> SqliteDatabase:
    """
    Initialize database connection.

    Returns:
        Database connection
    """
    db_path = get_db_path()
    logger.info(f"Initializing database at {db_path}")

    # Initialize database
    db.init(db_path)

    # Create tables if they don't exist
    db.create_tables([DownloadTask, DownloadedFile], safe=True)

    return db


def close_database() -> None:
    """Close database connection."""
    if not db.is_closed():
        logger.info("Closing database connection")
        db.close()
