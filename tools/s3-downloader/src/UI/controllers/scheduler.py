import datetime
import logging

from PySide6.QtCore import QObject, QTimer, Signal

logger = logging.getLogger(__name__)


class Scheduler(QObject):
    """Controller for scheduling downloads."""

    # Signals
    download_triggered = Signal()  # Emitted when it's time to download
    time_updated = Signal()  # Emitted when the next download time is updated
    download_skipped = Signal(str)  # Emitted when a download is skipped with reason

    def __init__(self, interval=3600):
        """
        Initialize the scheduler.

        Args:
            interval: Download interval in seconds
        """
        super().__init__()
        self.interval = interval
        self.next_download_time = None
        self.is_active = False

        # Create download timer
        self.download_timer = QTimer()
        self.download_timer.timeout.connect(self._on_download_timer)
        self.download_timer.setInterval(interval * 1000)  # Convert to milliseconds

        # Create time update timer (updates every second)
        self.time_update_timer = QTimer()
        self.time_update_timer.timeout.connect(self.time_updated)
        self.time_update_timer.setInterval(1000)  # 1 second

    def start(self, run_immediately=True):
        """
        Start the scheduler.

        Args:
            run_immediately: Whether to trigger a download immediately
        """
        if not self.is_active:
            self.is_active = True

            # Set next download time
            self._update_next_download_time()

            # Start timers
            self.download_timer.start()
            self.time_update_timer.start()

            logger.info(f"Scheduler started with {self.interval}s interval")

            # Trigger immediate download if requested
            if run_immediately:
                self.download_triggered.emit()

    def stop(self):
        """Stop the scheduler."""
        if self.is_active:
            self.is_active = False
            self.download_timer.stop()
            self.time_update_timer.stop()
            self.next_download_time = None
            logger.info("Scheduler stopped")

    def set_interval(self, interval):
        """
        Set the download interval.

        Args:
            interval: Download interval in seconds
        """
        self.interval = interval
        self.download_timer.setInterval(interval * 1000)  # Convert to milliseconds

        # Update next download time if active
        if self.is_active:
            self._update_next_download_time()

        logger.info(f"Scheduler interval set to {interval}s")

    def get_next_download_time(self):
        """Get the next scheduled download time."""
        return self.next_download_time

    def is_running(self):
        """Check if the scheduler is running."""
        return self.is_active

    def _on_download_timer(self):
        """Handle download timer timeout."""
        self.download_triggered.emit()
        self._update_next_download_time()

    def _update_next_download_time(self):
        """Update the next download time."""
        self.next_download_time = datetime.datetime.now() + datetime.timedelta(
            seconds=self.interval
        )

    def log_skip(self, reason="download in progress"):
        """
        Log that a download was skipped.

        Args:
            reason: Reason for skipping the download
        """
        logger.info(f"Scheduled download skipped: {reason}")
        self.download_skipped.emit(reason)
