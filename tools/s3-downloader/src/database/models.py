"""
Database models for S3 downloader.
"""

import datetime
from enum import Enum
from typing import Optional

from peewee import (
    CharField,
    DateTimeField,
    ForeignKeyField,
    IntegerField,
    Model,
    SqliteDatabase,
    TextField,
)

# Initialize database with None, will be set in __init__.py
db = SqliteDatabase(None)


class TaskStatus(Enum):
    """Enum for download task status."""

    IN_PROGRESS = "In Progress"
    COMPLETED = "Completed"
    FAILED = "Failed"
    ABORTED = "Aborted"


class FileStatus(Enum):
    """Enum for downloaded file status."""

    PENDING = "Pending"
    COMPLETED = "Completed"
    FAILED = "Failed"


class BaseModel(Model):
    """Base model for all database models."""

    class Meta:
        """Meta class for BaseModel."""

        database = db


class DownloadTask(BaseModel):
    """Model for download tasks."""

    start_time = DateTimeField(default=datetime.datetime.now)
    end_time = DateTimeField(null=True)
    status = CharField(default=TaskStatus.IN_PROGRESS.value)
    s3_prefix = CharField()
    local_path = CharField()
    total_files = IntegerField(default=0)
    successful_downloads = IntegerField(default=0)
    failed_downloads = IntegerField(default=0)
    error_message = TextField(null=True)

    def start(self) -> None:
        """Mark task as started."""
        self.status = TaskStatus.IN_PROGRESS.value
        self.save()

    def complete(self, successful: int, failed: int, error_message: Optional[str] = None) -> None:
        """
        Mark task as completed.

        Args:
            successful: Number of successfully downloaded files
            failed: Number of failed downloads
            error_message: Optional error message if there were failures
        """
        self.end_time = datetime.datetime.now()
        self.successful_downloads = successful
        self.failed_downloads = failed
        self.status = TaskStatus.COMPLETED.value if failed == 0 else TaskStatus.FAILED.value

        if failed > 0 and error_message:
            self.error_message = error_message

        self.save()

    def fail(self, error_message: Optional[str] = None) -> None:
        """
        Mark task as failed.

        Args:
            error_message: Optional error message describing the failure
        """
        self.end_time = datetime.datetime.now()
        self.status = TaskStatus.FAILED.value
        if error_message:
            self.error_message = error_message
        self.save()

    def abort(self, message: Optional[str] = None, successful: int = 0, failed: int = 0) -> None:
        """
        Mark task as aborted (stopped by user).

        Args:
            message: Optional message describing why the task was aborted
            successful: Number of successfully downloaded files before abort
            failed: Number of failed downloads before abort
        """
        self.end_time = datetime.datetime.now()
        self.status = TaskStatus.ABORTED.value
        # Record the download statistics even for aborted tasks
        self.successful_downloads = successful
        self.failed_downloads = failed

        if message:
            if self.error_message:
                self.error_message += f"\n\n{message}"
            else:
                self.error_message = message
        self.save()

    @property
    def duration(self) -> Optional[float]:
        """
        Get task duration in seconds.

        Returns:
            Duration in seconds or None if task is not completed
        """
        if (
            self.end_time
            and isinstance(self.end_time, datetime.datetime)
            and isinstance(self.start_time, datetime.datetime)
        ):
            return (self.end_time - self.start_time).total_seconds()
        return None

    def summarize_errors(self) -> str:
        """
        Summarize errors from all failed files in this task.

        Returns:
            A string containing a summary of all errors
        """
        if self.error_message:
            return str(self.error_message)

        # Get all failed files for this task
        failed_files = DownloadedFile.select().where(
            (DownloadedFile.task == self) & (DownloadedFile.status == FileStatus.FAILED.value)
        )

        if failed_files.count() == 0:
            return "No errors found"

        # Group similar errors and count them
        error_counts = {}
        for file in failed_files:
            if file.error_message:
                if file.error_message in error_counts:
                    error_counts[file.error_message] += 1
                else:
                    error_counts[file.error_message] = 1

        # Format the error summary
        summary = f"Total failed files: {failed_files.count()}\n\nError summary:\n"
        for error, count in error_counts.items():
            summary += f"- {error} ({count} files)\n"

        return summary

    @classmethod
    def get_recent_tasks(cls, limit: int = 10):
        """
        Get recent download tasks.

        Args:
            limit: Maximum number of tasks to return

        Returns:
            Query of recent tasks
        """
        return cls.select().order_by(cls.start_time.desc()).limit(limit)

    @classmethod
    def create_task(
        cls, s3_prefix: str, local_path: str, total_files: int, status: Optional[str] = None
    ):
        """
        Create a new download task.

        Args:
            s3_prefix: S3 prefix being downloaded
            local_path: Local path where files are being saved
            total_files: Total number of files to download
            status: Initial status (defaults to IN_PROGRESS)

        Returns:
            Created DownloadTask instance
        """
        if status is None:
            status = TaskStatus.IN_PROGRESS.value

        task = cls(
            s3_prefix=s3_prefix, local_path=local_path, total_files=total_files, status=status
        )
        task.save()
        return task


class DownloadedFile(BaseModel):
    """Model for downloaded files."""

    task = ForeignKeyField(DownloadTask, backref="files")
    s3_key = CharField()
    local_path = CharField()
    size = IntegerField(default=0)
    download_time = DateTimeField(null=True)
    status = CharField(default=FileStatus.PENDING.value)
    error_message = TextField(null=True)

    def complete(self, size: int) -> None:
        """
        Mark file as completed.

        Args:
            size: Size of the downloaded file in bytes
        """
        self.download_time = datetime.datetime.now()
        self.size = size
        self.status = FileStatus.COMPLETED.value
        self.save()

    def fail(self, error_message: str) -> None:
        """
        Mark file as failed.

        Args:
            error_message: Error message
        """
        self.download_time = datetime.datetime.now()
        self.status = FileStatus.FAILED.value
        self.error_message = error_message
        self.save()

    @classmethod
    def get_files_for_task(cls, task_id: int):
        """
        Get files for a specific task.

        Args:
            task_id: Task ID

        Returns:
            Query of files for the task
        """
        return cls.select().where(cls.task == task_id).order_by(cls.download_time)

    @classmethod
    def get_total_downloaded_count(cls) -> int:
        """
        Get total count of successfully downloaded files.

        Returns:
            Total number of downloaded files
        """
        return cls.select().where(cls.status == FileStatus.COMPLETED.value).count()

    @classmethod
    def create_file(
        cls,
        task,
        s3_key: str,
        local_path: str,
        status: Optional[str] = None,
        error_message: Optional[str] = None,
    ):
        """
        Create a new downloaded file record.

        Args:
            task: DownloadTask instance
            s3_key: S3 key of the file
            local_path: Local path where file is saved
            status: File status (defaults to PENDING)
            error_message: Optional error message

        Returns:
            Created DownloadedFile instance
        """
        if status is None:
            status = FileStatus.PENDING.value

        file_record = cls(
            task=task,
            s3_key=s3_key,
            local_path=local_path,
            status=status,
            error_message=error_message,
        )
        file_record.save()
        return file_record
