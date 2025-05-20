import logging
import shutil

from PySide6.QtCore import Signal
from PySide6.QtWidgets import QHBoxLayout, QLabel, QProgressBar, QWidget

logger = logging.getLogger(__name__)


class DiskSpaceWidget(QWidget):
    """Widget for displaying disk space information."""

    # Signal emitted when disk space is low
    low_space_warning = Signal(bool, float)  # warning active, free space ratio

    def __init__(self, parent=None, warning_threshold=0.1):
        """
        Initialize the disk space widget.

        Args:
            parent: Parent widget
            warning_threshold: Threshold for low space warning (0.0-1.0)
        """
        super().__init__(parent)
        self.warning_threshold = warning_threshold
        self._create_ui()

    def _create_ui(self):
        """Create the UI elements."""
        layout = QHBoxLayout(self)
        layout.setContentsMargins(0, 0, 0, 0)

        # Disk space label
        disk_space_label = QLabel("Disk Space:")
        layout.addWidget(disk_space_label)

        # Create progress bar for disk space
        self.disk_space_bar = QProgressBar()
        self.disk_space_bar.setRange(0, 100)
        self.disk_space_bar.setValue(0)
        self.disk_space_bar.setTextVisible(True)
        self.disk_space_bar.setFormat("%p% used")
        layout.addWidget(self.disk_space_bar, 1)  # 1 is stretch factor

        # Free space label
        self.free_space_label = QLabel("Free: N/A")
        layout.addWidget(self.free_space_label)

        # For compatibility with existing code, create empty labels
        self.total_space_label = QLabel()
        self.total_space_label.setVisible(False)
        self.used_space_label = QLabel()
        self.used_space_label.setVisible(False)

        # Warning label (hidden by default)
        self.space_warning_label = QLabel()
        self.space_warning_label.setStyleSheet("color: red; font-weight: bold;")
        self.space_warning_label.setVisible(False)
        layout.addWidget(self.space_warning_label)

    def update_disk_space_info(self, storage_path):
        """
        Update disk space information for the given storage path.

        Args:
            storage_path: Path to check disk space for

        Returns:
            tuple: (success, free_ratio) - success is True if space is sufficient
        """
        if not storage_path:
            # No path selected, clear disk space info
            self.disk_space_bar.setValue(0)
            self.free_space_label.setText("Free: N/A")
            self.space_warning_label.setVisible(False)
            return True, 1.0

        try:
            # Get disk usage information
            total, used, free = shutil.disk_usage(storage_path)

            # Calculate percentage used
            percent_used = int((used / total) * 100)

            # Update progress bar
            self.disk_space_bar.setValue(percent_used)

            # Format free space (in GB)
            free_gb = free / 1_000_000_000

            # Update free space label
            self.free_space_label.setText(f"Free: {free_gb:.1f} GB")

            # Check if free space is below warning threshold
            free_ratio = free / total
            if free_ratio < self.warning_threshold:
                # Show warning message
                self.space_warning_label.setText("Low space!")
                self.space_warning_label.setVisible(True)
                self.low_space_warning.emit(True, free_ratio)
                return False, free_ratio
            else:
                self.space_warning_label.setVisible(False)
                self.low_space_warning.emit(False, free_ratio)
                return True, free_ratio

        except Exception as e:
            logger.error(f"Error getting disk space info: {e}")
            # Clear disk space info on error
            self.disk_space_bar.setValue(0)
            self.free_space_label.setText("Free: Error")
            self.space_warning_label.setVisible(False)
            return True, 1.0  # Assume space is OK on error

    def set_warning_threshold(self, threshold):
        """Set the warning threshold."""
        self.warning_threshold = threshold
