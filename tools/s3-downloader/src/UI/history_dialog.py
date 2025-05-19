"""
Download history dialog for the S3 downloader application.
"""

import logging
from typing import Optional

from PySide6.QtCore import Qt, Slot
from PySide6.QtGui import QColor
from PySide6.QtWidgets import (
    QAbstractItemView,
    QDialog,
    QHBoxLayout,
    QHeaderView,
    QLabel,
    QPushButton,
    QTableWidget,
    QTableWidgetItem,
    QVBoxLayout,
    QWidget,
)

from .s3_controller import S3Controller

logger = logging.getLogger(__name__)


class HistoryDialog(QDialog):
    """Dialog for displaying download history."""

    def __init__(
        self, parent: Optional[QWidget] = None, s3_controller: Optional[S3Controller] = None
    ):
        """
        Initialize the history dialog.

        Args:
            parent: Parent widget
            s3_controller: S3 controller instance
        """
        super().__init__(parent)
        self.s3_controller = s3_controller or S3Controller()
        self.selected_task_id = None

        self.setWindowTitle("Download History")
        self.setMinimumSize(800, 500)

        # Create layout
        self.main_layout = QVBoxLayout(self)

        # Create history table
        self.create_history_table()

        # Create task error section
        self.create_task_error_section()

        # Create details section
        self.create_details_section()

        # Create buttons
        self.create_buttons()

        # Load history data
        self.load_history()

    def create_history_table(self):
        """Create history table."""
        # Create table
        self.history_table = QTableWidget()
        self.history_table.setColumnCount(6)
        self.history_table.setHorizontalHeaderLabels(
            ["ID", "Start Time", "Status", "Files", "Success/Failed", "Path"]
        )
        self.history_table.setSelectionBehavior(QAbstractItemView.SelectionBehavior.SelectRows)
        self.history_table.setSelectionMode(QAbstractItemView.SelectionMode.SingleSelection)
        self.history_table.setEditTriggers(QAbstractItemView.EditTrigger.NoEditTriggers)
        self.history_table.setAlternatingRowColors(True)
        self.history_table.horizontalHeader().setSectionResizeMode(QHeaderView.ResizeMode.Stretch)
        self.history_table.verticalHeader().setVisible(False)
        self.history_table.setShowGrid(True)
        self.history_table.setSortingEnabled(True)
        self.history_table.selectionModel().selectionChanged.connect(self.on_task_selected)

        # Add table to layout
        self.main_layout.addWidget(self.history_table)

    def create_task_error_section(self):
        """Create task error section."""
        # Create task error layout
        task_error_layout = QVBoxLayout()

        # Create task error header
        task_error_header = QLabel("Task Error Summary")
        task_error_header.setStyleSheet("font-weight: bold; font-size: 14px;")
        task_error_layout.addWidget(task_error_header)

        # Create task error text area
        self.task_error_label = QLabel()
        self.task_error_label.setWordWrap(True)
        self.task_error_label.setStyleSheet(
            "background-color: #f8f8f8; padding: 10px; border: 1px solid #ddd;"
        )
        self.task_error_label.setMinimumHeight(60)
        self.task_error_label.setTextFormat(Qt.TextFormat.PlainText)
        task_error_layout.addWidget(self.task_error_label)

        # Add task error layout to main layout
        self.main_layout.addLayout(task_error_layout)

    def create_details_section(self):
        """Create details section."""
        # Create details layout
        details_layout = QVBoxLayout()

        # Create details header
        details_header = QLabel("Download Details")
        details_header.setStyleSheet("font-weight: bold; font-size: 14px;")
        details_layout.addWidget(details_header)

        # Create details table
        self.details_table = QTableWidget()
        self.details_table.setColumnCount(5)
        self.details_table.setHorizontalHeaderLabels(
            ["File", "Status", "Size", "Download Time", "Error"]
        )
        self.details_table.setSelectionBehavior(QAbstractItemView.SelectionBehavior.SelectRows)
        self.details_table.setEditTriggers(QAbstractItemView.EditTrigger.NoEditTriggers)
        self.details_table.setAlternatingRowColors(True)
        self.details_table.horizontalHeader().setSectionResizeMode(QHeaderView.ResizeMode.Stretch)
        self.details_table.verticalHeader().setVisible(False)
        self.details_table.setShowGrid(True)
        self.details_table.setSortingEnabled(True)
        details_layout.addWidget(self.details_table)

        # Add details layout to main layout
        self.main_layout.addLayout(details_layout)

    def create_buttons(self):
        """Create buttons."""
        # Create buttons layout
        buttons_layout = QHBoxLayout()

        # Create refresh button
        refresh_button = QPushButton("Refresh")
        refresh_button.clicked.connect(self.load_history)
        buttons_layout.addWidget(refresh_button)

        # Create close button
        close_button = QPushButton("Close")
        close_button.clicked.connect(self.accept)
        buttons_layout.addWidget(close_button)

        # Add buttons layout to main layout
        self.main_layout.addLayout(buttons_layout)

    def load_history(self):
        """Load download history."""
        # Clear table
        self.history_table.setRowCount(0)

        # Clear task error label
        self.task_error_label.setText("")
        self.task_error_label.setStyleSheet(
            "background-color: #f8f8f8; padding: 10px; border: 1px solid #ddd; color: #666;"
        )

        # Get history data
        tasks = self.s3_controller.get_download_history(limit=50)

        # Add data to table
        for i, task in enumerate(tasks):
            self.history_table.insertRow(i)

            # ID
            task_id = getattr(task, "id", 0)
            id_item = QTableWidgetItem(str(task_id))
            id_item.setData(Qt.ItemDataRole.UserRole, task_id)
            self.history_table.setItem(i, 0, id_item)

            # Start Time
            start_time = getattr(task, "start_time", None)
            start_time_str = start_time.strftime("%Y-%m-%d %H:%M:%S") if start_time else "N/A"
            self.history_table.setItem(i, 1, QTableWidgetItem(start_time_str))

            # Status
            status = getattr(task, "status", "Unknown")
            status_item = QTableWidgetItem(str(status))
            if status == "Completed":
                status_item.setForeground(QColor("darkgreen"))
            elif status == "Failed":
                status_item.setForeground(QColor("red"))
            self.history_table.setItem(i, 2, status_item)

            # Files
            files_count = getattr(task, "total_files", 0)
            self.history_table.setItem(i, 3, QTableWidgetItem(str(files_count)))

            # Success/Failed
            successful = getattr(task, "successful_downloads", 0)
            failed = getattr(task, "failed_downloads", 0)
            success_failed = f"{successful}/{failed}"
            self.history_table.setItem(i, 4, QTableWidgetItem(success_failed))

            # Path
            local_path = getattr(task, "local_path", "")
            self.history_table.setItem(i, 5, QTableWidgetItem(str(local_path)))

        # Sort by start time (newest first)
        self.history_table.sortItems(1, Qt.SortOrder.DescendingOrder)

    @Slot()
    def on_task_selected(self):
        """Handle task selection."""
        selected_rows = self.history_table.selectionModel().selectedRows()
        if not selected_rows:
            return

        # Get task ID
        row = selected_rows[0].row()
        item = self.history_table.item(row, 0)
        if item:
            task_id = item.data(Qt.ItemDataRole.UserRole)
            if task_id:
                self.selected_task_id = int(task_id)
                # Load task details
                self.load_task_details(self.selected_task_id)

    def load_task_details(self, task_id: int) -> None:
        """
        Load task details.

        Args:
            task_id: Task ID
        """
        # Clear table
        self.details_table.setRowCount(0)

        # Get task details
        details = self.s3_controller.get_download_details(task_id)
        task = details.get("task")
        files = details.get("files", [])

        # Display task error message if available
        if task and hasattr(task, "error_message") and task.error_message:
            self.task_error_label.setText(task.error_message)
            self.task_error_label.setStyleSheet(
                "background-color: #fff0f0; padding: 10px; border: 1px solid #ffcccc; color: #cc0000;"
            )
        elif task and hasattr(task, "summarize_errors"):
            # Use the summarize_errors method if available
            error_summary = task.summarize_errors()
            if error_summary and error_summary != "No errors found":
                self.task_error_label.setText(error_summary)
                self.task_error_label.setStyleSheet(
                    "background-color: #fff0f0; padding: 10px; border: 1px solid #ffcccc; color: #cc0000;"
                )
            else:
                self.task_error_label.setText("No task-level errors reported.")
                self.task_error_label.setStyleSheet(
                    "background-color: #f0fff0; padding: 10px; border: 1px solid #ccffcc; color: #006600;"
                )
        else:
            self.task_error_label.setText("No task-level error information available.")
            self.task_error_label.setStyleSheet(
                "background-color: #f8f8f8; padding: 10px; border: 1px solid #ddd; color: #666;"
            )

        # Add data to table
        for i, file in enumerate(files):
            self.details_table.insertRow(i)

            # File
            s3_key = getattr(file, "s3_key", "")
            file_name = s3_key.split("/")[-1] if "/" in s3_key else s3_key
            self.details_table.setItem(i, 0, QTableWidgetItem(file_name))

            # Status
            status = getattr(file, "status", "Unknown")
            status_item = QTableWidgetItem(str(status))
            if status == "Completed":
                status_item.setForeground(QColor("darkgreen"))
            elif status == "Failed":
                status_item.setForeground(QColor("red"))
            self.details_table.setItem(i, 1, status_item)

            # Size
            size = getattr(file, "size", 0)
            size_str = f"{size / 1024:.1f} KB" if size > 0 else "N/A"
            self.details_table.setItem(i, 2, QTableWidgetItem(size_str))

            # Download Time
            download_time = getattr(file, "download_time", None)
            download_time_str = (
                download_time.strftime("%Y-%m-%d %H:%M:%S") if download_time else "N/A"
            )
            self.details_table.setItem(i, 3, QTableWidgetItem(download_time_str))

            # Error
            error_msg = getattr(file, "error_message", "") or ""
            self.details_table.setItem(i, 4, QTableWidgetItem(error_msg))
