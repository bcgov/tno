import logging
from typing import Optional

from PySide6.QtCore import Qt, Slot
from PySide6.QtGui import QColor
from PySide6.QtWidgets import (
    QAbstractItemView,
    QGroupBox,
    QHBoxLayout,
    QHeaderView,
    QLabel,
    QPushButton,
    QTableWidget,
    QTableWidgetItem,
    QVBoxLayout,
    QWidget,
)

from ..s3_controller import S3Controller

logger = logging.getLogger(__name__)

# Define style constants
TITLE_STYLE = "font-weight: bold; font-size: 16px;"
HEADER_STYLE = "font-weight: bold; font-size: 14px;"
ERROR_STYLE = "background-color: #fff0f0; padding: 10px; border: 1px solid #ffcccc; color: #cc0000;"
SUCCESS_STYLE = (
    "background-color: #f0fff0; padding: 10px; border: 1px solid #ccffcc; color: #006600;"
)
DEFAULT_STYLE = "background-color: #f8f8f8; padding: 10px; border: 1px solid #ddd; color: #666;"


class HistoryWidget(QWidget):
    """Widget for displaying download history."""

    def __init__(
        self, parent: Optional[QWidget] = None, s3_controller: Optional[S3Controller] = None
    ):
        """
        Initialize the history widget.

        Args:
            parent: Parent widget
            s3_controller: S3 controller instance
        """
        super().__init__(parent)
        self.s3_controller = s3_controller or S3Controller()
        self.selected_task_id = None

        # Create layout
        self.main_layout = QVBoxLayout(self)
        self.main_layout.setContentsMargins(0, 0, 0, 0)

        # Create title
        title_label = QLabel("Download History")
        title_label.setStyleSheet(TITLE_STYLE)
        self.main_layout.addWidget(title_label)

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
        # Create group box
        history_group = QGroupBox("Recent Downloads")
        history_layout = QVBoxLayout()

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
        self.history_table.horizontalHeader().setStretchLastSection(True)

        self.history_table.verticalHeader().setVisible(False)
        self.history_table.setShowGrid(True)
        self.history_table.setSortingEnabled(True)
        self.history_table.selectionModel().selectionChanged.connect(self.on_task_selected)
        history_layout.addWidget(self.history_table)

        history_group.setLayout(history_layout)
        self.main_layout.addWidget(history_group)

    def create_task_error_section(self):
        """Create task error section."""
        # Create task error layout
        task_error_layout = QVBoxLayout()

        # Create task error header
        task_error_header = QLabel("Task Error Summary")
        task_error_header.setStyleSheet(HEADER_STYLE)
        task_error_layout.addWidget(task_error_header)

        # Create task error text area
        self.task_error_label = QLabel()
        self.task_error_label.setWordWrap(True)
        self.task_error_label.setStyleSheet(DEFAULT_STYLE)
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
        details_header.setStyleSheet(HEADER_STYLE)
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

        # Create status label
        self.status_label = QLabel("Last updated: Never")
        buttons_layout.addWidget(self.status_label, 1)  # 1 is stretch factor

        # Create refresh button
        refresh_button = QPushButton("Refresh")
        refresh_button.clicked.connect(self.load_history)
        buttons_layout.addWidget(refresh_button)

        # Add buttons layout to main layout
        self.main_layout.addLayout(buttons_layout)

    def load_history(self):
        """Load download history."""
        # Clear table
        self.history_table.setRowCount(0)

        # Clear task error label
        self.task_error_label.setText("")
        self.task_error_label.setStyleSheet(DEFAULT_STYLE)

        # Update status label with current time
        import datetime

        now = datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        self.status_label.setText(f"Last updated: {now}")

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
                status_item.setForeground(QColor(0, 100, 0))  # Dark green
            elif status == "Failed":
                status_item.setForeground(QColor(255, 0, 0))  # Red
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

        error_text = "No task-level error information available."
        error_style = DEFAULT_STYLE

        if task:
            if hasattr(task, "error_message") and task.error_message:
                error_text = task.error_message
                error_style = ERROR_STYLE
            elif hasattr(task, "summarize_errors"):
                error_summary = task.summarize_errors()
                if error_summary and error_summary != "No errors found":
                    error_text = error_summary
                    error_style = ERROR_STYLE
                else:
                    error_text = "No task-level errors reported."
                    error_style = SUCCESS_STYLE

        self.task_error_label.setText(error_text)
        self.task_error_label.setStyleSheet(error_style)

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
                status_item.setForeground(QColor(0, 100, 0))  # Dark green
            elif status == "Failed":
                status_item.setForeground(QColor(255, 0, 0))  # Red
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
