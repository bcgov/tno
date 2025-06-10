import logging
from pathlib import Path

from PySide6.QtCore import Signal
from PySide6.QtWidgets import QFileDialog, QHBoxLayout, QLabel, QLineEdit, QPushButton, QWidget

logger = logging.getLogger(__name__)


class StoragePathWidget(QWidget):
    """Widget for selecting storage path."""

    # Signal emitted when storage path changes
    path_changed = Signal(str)

    def __init__(self, parent=None):
        """Initialize the storage path widget."""
        super().__init__(parent)
        self.storage_path = ""
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

    def _browse_storage_path(self):
        """Open file dialog to select storage path."""
        directory = QFileDialog.getExistingDirectory(
            self, "Select Storage Directory", self.storage_path_input.text() or str(Path.home())
        )
        if directory:
            self.set_storage_path(directory)
            logger.info(f"Selected storage path: {directory}")

    def set_storage_path(self, path):
        """Set the storage path."""
        self.storage_path = path
        self.storage_path_input.setText(path)
        self.path_changed.emit(path)

    def get_storage_path(self):
        """Get the current storage path."""
        return self.storage_path_input.text().strip()
