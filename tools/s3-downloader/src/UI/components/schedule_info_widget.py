import datetime
import logging

from PySide6.QtWidgets import QGroupBox, QLabel, QVBoxLayout, QWidget

logger = logging.getLogger(__name__)


class ScheduleInfoWidget(QWidget):
    """Widget for displaying schedule information."""

    def __init__(self, parent=None, interval=3600):
        """
        Initialize the schedule info widget.

        Args:
            parent: Parent widget
            interval: Download interval in seconds
        """
        super().__init__(parent)
        self.interval = interval
        self.next_download_time = None
        self._create_ui()

    def _create_ui(self):
        """Create the UI elements."""
        layout = QVBoxLayout(self)
        layout.setContentsMargins(0, 0, 0, 0)

        # Create group box for schedule info
        schedule_group = QGroupBox("Download Schedule")
        schedule_layout = QVBoxLayout()

        # Status label
        self.schedule_status_label = QLabel("Automatic download is not running")
        self.schedule_status_label.setStyleSheet("font-weight: bold;")
        schedule_layout.addWidget(self.schedule_status_label)

        # Next download time label
        self.next_download_label = QLabel("Next download: Not scheduled")
        schedule_layout.addWidget(self.next_download_label)

        # Interval label
        self.interval_label = QLabel(f"Download interval: {self.interval} seconds")
        schedule_layout.addWidget(self.interval_label)

        schedule_group.setLayout(schedule_layout)
        layout.addWidget(schedule_group)

    def set_interval(self, interval):
        """
        Set the download interval.

        Args:
            interval: Download interval in seconds
        """
        self.interval = interval
        self.interval_label.setText(f"Download interval: {self.interval} seconds")

    def set_schedule_active(self, active, failed=False):
        """
        Set the schedule status.

        Args:
            active: True if schedule is active, False otherwise
            failed: True if last download attempt failed
        """
        if active:
            status = "Automatic download is running"
            if failed:
                status += " (last attempt failed)"
            self.schedule_status_label.setText(status)
        else:
            self.schedule_status_label.setText("Automatic download is not running")
            self.next_download_label.setText("Next download: Not scheduled")
            self.next_download_time = None

    def set_schedule_paused(self, reason):
        """
        Set the schedule status to paused with a reason.

        Args:
            reason: Reason for pausing
        """
        self.schedule_status_label.setText(f"Automatic download stopped ({reason})")
        self.next_download_label.setText("Next download: Not scheduled")
        self.next_download_time = None

    def set_next_download_time(self, next_time):
        """
        Set the next download time.

        Args:
            next_time: Next download time as datetime object
        """
        self.next_download_time = next_time

    def update_next_download_time(self):
        """Update the next download time display."""
        if self.next_download_time:
            # Calculate remaining time
            now = datetime.datetime.now()
            if now < self.next_download_time:
                # Format time as HH:MM:SS
                time_str = self.next_download_time.strftime("%H:%M:%S")

                # Calculate and format remaining time
                time_diff = self.next_download_time - now
                hours, remainder = divmod(time_diff.seconds, 3600)
                minutes, seconds = divmod(remainder, 60)
                remaining_str = f"{hours:02d}:{minutes:02d}:{seconds:02d}"

                # Update label
                self.next_download_label.setText(f"Next download: {time_str} (in {remaining_str})")
            else:
                # Time has passed, this should be updated soon by the download task
                self.next_download_label.setText("Next download: Executing now...")
        else:
            # No scheduled download
            self.next_download_label.setText("Next download: Not scheduled")
