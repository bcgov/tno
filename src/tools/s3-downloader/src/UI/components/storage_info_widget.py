"""
Storage information widget for the S3 downloader application.
Combines storage path selection and disk space information.
"""

import logging
import shutil
from pathlib import Path

from PySide6.QtCore import Signal
from PySide6.QtWidgets import QFileDialog, QHBoxLayout, QLabel, QLineEdit, QPushButton, QWidget

logger = logging.getLogger(__name__)


class StorageInfoWidget(QWidget):
    """Widget for selecting storage path and displaying disk space information."""

    # Signal emitted when storage path changes
    path_changed = Signal(str)

    # Signal emitted when disk space is low
    low_space_warning = Signal(bool, float)  # warning active, free space ratio

    def __init__(self, parent=None, warning_threshold=0.1):
        """
        Initialize the storage info widget.

        Args:
            parent: Parent widget
            warning_threshold: Threshold for low space warning (0.0-1.0)
        """
        super().__init__(parent)
        self.storage_path = ""
        self.warning_threshold = warning_threshold
        self._create_ui()

    def _create_ui(self):
        """Create the UI elements."""
        layout = QHBoxLayout(self)
        layout.setContentsMargins(0, 0, 0, 0)

        # Storage path label
        path_label = QLabel("Storage Path:")
        layout.addWidget(path_label)

        # Storage path input
        self.storage_path_input = QLineEdit()
        self.storage_path_input.setPlaceholderText("Select local storage path")
        layout.addWidget(self.storage_path_input, 1)  # 1 is stretch factor

        # Browse button
        browse_btn = QPushButton("Browse...")
        browse_btn.clicked.connect(self._browse_storage_path)
        layout.addWidget(browse_btn)

        # Space separator
        layout.addSpacing(10)

        # Free space label
        self.free_space_label = QLabel("Free: N/A")
        layout.addWidget(self.free_space_label)

        # Warning label (hidden by default)
        self.space_warning_label = QLabel("Low space!")
        self.space_warning_label.setStyleSheet("color: red; font-weight: bold;")
        self.space_warning_label.setVisible(False)
        layout.addWidget(self.space_warning_label)

    def _browse_storage_path(self):
        """Open file dialog to select storage path."""
        directory = QFileDialog.getExistingDirectory(
            self, "Select Storage Directory", self.storage_path_input.text() or str(Path.home())
        )
        if directory:
            self.set_storage_path(directory)
            logger.info(f"Selected storage path: {directory}")

    def set_storage_path(self, path):
        """Set the storage path and update disk space info."""
        self.storage_path = path
        self.storage_path_input.setText(path)
        self.update_disk_space_info()
        self.path_changed.emit(path)

    def get_storage_path(self):
        """Get the current storage path."""
        return self.storage_path_input.text().strip()

    def update_disk_space_info(self):
        """
        Update disk space information for the current storage path.

        Returns:
            tuple: (success, free_ratio) - success is True if space is sufficient
        """
        storage_path = self.get_storage_path()

        if not storage_path:
            # No path selected, clear disk space info
            self.free_space_label.setText("Free: N/A")
            self.space_warning_label.setVisible(False)
            return True, 1.0

        try:
            # Get disk usage information
            total, used, free = shutil.disk_usage(storage_path)

            # Calculate free space ratio
            free_ratio = free / total

            # Format free space (in GB)
            free_gb = free / 1_000_000_000

            # Update free space label
            self.free_space_label.setText(f"Free: {free_gb:.1f} GB ({free_ratio:.1%})")

            # Check if free space is below warning threshold
            if free_ratio < self.warning_threshold:
                # Show warning message
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
            self.free_space_label.setText("Free: Error")
            self.space_warning_label.setVisible(False)
            return True, 1.0  # Assume space is OK on error

    def set_warning_threshold(self, threshold):
        """Set the warning threshold."""
        self.warning_threshold = threshold
        self.update_disk_space_info()  # Update to apply new threshold
