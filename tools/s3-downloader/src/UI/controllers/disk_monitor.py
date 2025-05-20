import logging
import shutil
from pathlib import Path

from PySide6.QtCore import QObject, QTimer, Signal

logger = logging.getLogger(__name__)


class DiskMonitor(QObject):
    """Controller for monitoring disk space."""

    # Signals
    space_status_changed = Signal(bool, float, str)  # is_ok, free_ratio, message

    def __init__(self, warning_threshold=0.1, update_interval=10000):
        """
        Initialize the disk monitor.

        Args:
            warning_threshold: Threshold for low space warning (0.0-1.0)
            update_interval: Update interval in milliseconds
        """
        super().__init__()
        self.warning_threshold = warning_threshold
        self.update_interval = update_interval
        self.storage_path = ""

        # Create timer for periodic updates
        self.timer = QTimer()
        self.timer.timeout.connect(self.check_disk_space)
        self.timer.setInterval(update_interval)

    def start_monitoring(self, storage_path):
        """
        Start monitoring disk space.

        Args:
            storage_path: Path to monitor
        """
        self.storage_path = storage_path
        if storage_path:
            self.timer.start()
            self.check_disk_space()  # Initial check
        else:
            self.timer.stop()

    def stop_monitoring(self):
        """Stop monitoring disk space."""
        self.timer.stop()

    def set_warning_threshold(self, threshold):
        """
        Set the warning threshold.

        Args:
            threshold: New threshold value (0.0-1.0)
        """
        self.warning_threshold = threshold

    def check_disk_space(self):
        """Check disk space and emit signal if necessary."""
        if not self.storage_path:
            return True, 1.0, "No storage path set"

        try:
            # Ensure path exists
            path = Path(self.storage_path)
            if not path.exists():
                path.mkdir(parents=True, exist_ok=True)

            # Get disk usage information
            total, used, free = shutil.disk_usage(self.storage_path)

            # Calculate free space ratio
            free_ratio = free / total
            free_gb = free / 1_000_000_000

            # Check if free space is below warning threshold
            if free_ratio < self.warning_threshold:
                message = f"Low disk space! Only {free_ratio:.1%} available ({free_gb:.1f} GB free)"
                self.space_status_changed.emit(False, free_ratio, message)
                logger.warning(message)
                return False, free_ratio, message
            else:
                message = f"Disk space OK: {free_ratio:.1%} available ({free_gb:.1f} GB free)"
                self.space_status_changed.emit(True, free_ratio, message)
                return True, free_ratio, message

        except Exception as e:
            message = f"Error checking disk space: {e}"
            logger.error(message)
            self.space_status_changed.emit(True, 1.0, message)  # Assume OK on error
            return True, 1.0, message
