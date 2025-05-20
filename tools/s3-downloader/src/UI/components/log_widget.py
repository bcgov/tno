import logging

from PySide6.QtCore import Slot
from PySide6.QtWidgets import QGroupBox, QProgressBar, QPushButton, QTextEdit, QVBoxLayout, QWidget

logger = logging.getLogger(__name__)


class LogWidget(QWidget):
    """Widget for displaying log messages and progress."""

    def __init__(self, parent=None):
        """Initialize the log widget."""
        super().__init__(parent)
        self._create_ui()

    def _create_ui(self):
        """Create the UI elements."""
        layout = QVBoxLayout(self)
        layout.setContentsMargins(0, 0, 0, 0)

        # Create group box
        log_group = QGroupBox("Log")
        log_layout = QVBoxLayout()

        # Log text area
        self.log_text = QTextEdit()
        self.log_text.setReadOnly(True)
        log_layout.addWidget(self.log_text)

        # Progress bar
        self.progress_bar = QProgressBar()
        self.progress_bar.setRange(0, 100)
        self.progress_bar.setValue(0)
        log_layout.addWidget(self.progress_bar)

        # Clear log button
        self.clear_log_btn = QPushButton("Clear Log")
        self.clear_log_btn.clicked.connect(self.clear_log)
        log_layout.addWidget(self.clear_log_btn)

        log_group.setLayout(log_layout)
        layout.addWidget(log_group)

    @Slot()
    def clear_log(self):
        """Clear the log text area."""
        self.log_text.clear()
        self.progress_bar.setValue(0)
        logger.info("Log cleared")

    def log_message(self, message):
        """
        Add a message to the log text area.

        Args:
            message: Message to log
        """
        self.log_text.append(message)
        # Auto scroll to bottom
        self.log_text.verticalScrollBar().setValue(self.log_text.verticalScrollBar().maximum())
        logger.info(message)

    def set_progress(self, progress):
        """
        Set the progress bar value.

        Args:
            progress: Progress value (0-100)
        """
        self.progress_bar.setValue(progress)

    def reset_progress(self):
        """Reset the progress bar to 0."""
        self.progress_bar.setValue(0)
