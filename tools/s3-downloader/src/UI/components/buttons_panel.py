import logging

from PySide6.QtCore import Signal
from PySide6.QtWidgets import QHBoxLayout, QPushButton, QWidget

logger = logging.getLogger(__name__)


class ButtonsPanel(QWidget):
    """Panel with main action buttons."""

    # Signals
    test_connection_clicked = Signal()
    download_clicked = Signal()

    def __init__(self, parent=None):
        """Initialize the buttons panel."""
        super().__init__(parent)
        self.download_active = False
        self._create_ui()

    def _create_ui(self):
        """Create the UI elements."""
        layout = QHBoxLayout(self)
        layout.setContentsMargins(0, 0, 0, 0)

        # Test connection button
        self.test_connection_btn = QPushButton("Test Connection")
        self.test_connection_btn.setMinimumHeight(40)
        self.test_connection_btn.setStyleSheet("font-size: 12px; font-weight: bold;")
        self.test_connection_btn.clicked.connect(self.test_connection_clicked)
        layout.addWidget(self.test_connection_btn)

        # Download button
        self.download_btn = QPushButton("Start Download")
        self.download_btn.setMinimumHeight(40)
        self.download_btn.setStyleSheet("font-size: 12px; font-weight: bold;")
        self.download_btn.clicked.connect(self._on_download_clicked)
        layout.addWidget(self.download_btn)

    def _on_download_clicked(self):
        """Handle download button click."""
        self.download_clicked.emit()

    def set_download_active(self, active):
        """
        Set the download button state.

        Args:
            active: True if download is active, False otherwise
        """
        self.download_active = active
        self.download_btn.setText("Stop Download" if active else "Start Download")

    def set_test_connection_enabled(self, enabled):
        """
        Enable or disable the test connection button.

        Args:
            enabled: True to enable, False to disable
        """
        self.test_connection_btn.setEnabled(enabled)

    def is_download_active(self):
        """Check if download is active."""
        return self.download_active
