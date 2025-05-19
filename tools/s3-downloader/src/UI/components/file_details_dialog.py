"""
File details dialog for displaying download file information.
"""

from PySide6.QtGui import QColor
from PySide6.QtWidgets import (
    QComboBox,
    QDialog,
    QHBoxLayout,
    QHeaderView,
    QLabel,
    QLineEdit,
    QPushButton,
    QTableWidget,
    QTableWidgetItem,
    QVBoxLayout,
)

from .numeric_table_widget_item import NumericTableWidgetItem


class FileDetailsDialog(QDialog):
    """Dialog for displaying file download details with pagination and filtering."""

    def __init__(self, parent=None):
        """Initialize the dialog."""
        super().__init__(parent)
        self.setWindowTitle("File Download Details")
        self.setMinimumSize(1000, 700)

        self.current_page = 1
        self.page_size = 200
        self.total_files = 0
        self.task_id = None
        self.all_files = []
        self.filtered_files = []

        self.setup_ui()

    def setup_ui(self):
        """Set up the user interface."""
        layout = QVBoxLayout(self)

        # Search and filter bar
        filter_layout = QHBoxLayout()

        self.search_edit = QLineEdit()
        self.search_edit.setPlaceholderText("Search files...")
        self.search_edit.textChanged.connect(self.apply_filters)
        filter_layout.addWidget(QLabel("Search:"))
        filter_layout.addWidget(self.search_edit)

        self.status_filter = QComboBox()
        self.status_filter.addItem("All Status", "")
        self.status_filter.addItem("Success", "Completed")
        self.status_filter.addItem("Failed", "Failed")
        self.status_filter.addItem("Pending", "Pending")
        self.status_filter.currentIndexChanged.connect(self.apply_filters)
        filter_layout.addWidget(QLabel("Status:"))
        filter_layout.addWidget(self.status_filter)

        layout.addLayout(filter_layout)

        # File table
        self.table = QTableWidget()
        self.table.setColumnCount(5)
        self.table.setHorizontalHeaderLabels(
            ["File", "Status", "Size", "Download Time", "Error Message"]
        )
        self.table.horizontalHeader().setSectionResizeMode(QHeaderView.ResizeMode.Stretch)
        self.table.verticalHeader().setVisible(False)
        self.table.setAlternatingRowColors(True)
        self.table.setSortingEnabled(True)
        self.table.setSelectionBehavior(self.table.SelectionBehavior.SelectRows)
        self.table.setSelectionMode(self.table.SelectionMode.SingleSelection)
        layout.addWidget(self.table)

        # Pagination controls
        page_layout = QHBoxLayout()
        page_layout.addStretch()

        self.prev_btn = QPushButton("Previous")
        self.prev_btn.clicked.connect(self.prev_page)
        page_layout.addWidget(self.prev_btn)

        self.page_label = QLabel("Page 1")
        page_layout.addWidget(self.page_label)

        self.next_btn = QPushButton("Next")
        self.next_btn.clicked.connect(self.next_page)
        page_layout.addWidget(self.next_btn)

        self.status_label = QLabel("Total: 0 files")
        page_layout.addWidget(self.status_label)

        layout.addLayout(page_layout)

        # Close button
        close_btn = QPushButton("Close")
        close_btn.clicked.connect(self.accept)
        layout.addWidget(close_btn)

        self.update_buttons()

    def load_files(self, task_id, files):
        """
        Load files data into the dialog.
        Args:
            task_id: ID of the task
            files: List of file objects
        """
        self.task_id = task_id
        self.all_files = files
        self.total_files = len(files)
        self.current_page = 1

        # Reset UI elements
        self.search_edit.setText("")
        self.status_filter.setCurrentIndex(0)  # "All Status"

        self.apply_filters()

    def apply_filters(self):
        """Apply filters and update the table."""
        search_text = self.search_edit.text().lower()
        status_filter = self.status_filter.currentData()

        # Apply filters
        self.filtered_files = []
        for file in self.all_files:
            # Check status filter
            if status_filter and getattr(file, "status", "") != status_filter:
                continue

            # Check search text
            s3_key = getattr(file, "s3_key", "").lower()
            if search_text and search_text not in s3_key:
                continue

            self.filtered_files.append(file)

        self.total_files = len(self.filtered_files)
        self.current_page = 1
        self.update_table()

    def update_table(self):
        """Update the table with current page data."""
        self.table.setRowCount(0)

        if not self.filtered_files:
            self.status_label.setText("No matching files found")
            self.update_buttons()
            return

        # Calculate pagination
        start_idx = (self.current_page - 1) * self.page_size
        end_idx = min(start_idx + self.page_size, len(self.filtered_files))
        page_files = self.filtered_files[start_idx:end_idx]

        # Add files to table
        for row, file in enumerate(page_files):
            self.table.insertRow(row)

            # File name
            s3_key = getattr(file, "s3_key", "")
            file_name = s3_key.split("/")[-1] if "/" in s3_key else s3_key
            self.table.setItem(row, 0, QTableWidgetItem(file_name))

            # Status
            status = getattr(file, "status", "Unknown")
            status_item = QTableWidgetItem(str(status))
            if status == "Completed":
                status_item.setForeground(QColor(0, 100, 0))  # Dark green
            elif status == "Failed":
                status_item.setForeground(QColor(255, 0, 0))  # Red
            self.table.setItem(row, 1, status_item)

            # Size
            size = getattr(file, "size", 0)
            size_str = f"{size / 1024:.1f} KB" if size > 0 else "N/A"
            size_item = NumericTableWidgetItem(size_str, size if size > 0 else 0)
            self.table.setItem(row, 2, size_item)

            # Download Time
            download_time = getattr(file, "download_time", None)
            download_time_str = (
                download_time.strftime("%Y-%m-%d %H:%M:%S") if download_time else "N/A"
            )
            self.table.setItem(row, 3, QTableWidgetItem(download_time_str))

            # Error
            error_msg = getattr(file, "error_message", "") or ""
            self.table.setItem(row, 4, QTableWidgetItem(error_msg))

        # Update status
        self.status_label.setText(
            f"Total: {len(self.filtered_files)} files, showing {start_idx + 1}-{end_idx}"
        )
        self.update_buttons()

    def update_buttons(self):
        """Update pagination buttons state."""
        total_pages = max(1, (len(self.filtered_files) + self.page_size - 1) // self.page_size)
        self.prev_btn.setEnabled(self.current_page > 1)
        self.next_btn.setEnabled(self.current_page < total_pages)
        self.page_label.setText(f"Page {self.current_page}/{total_pages}")

    def prev_page(self):
        """Go to previous page."""
        if self.current_page > 1:
            self.current_page -= 1
            self.update_table()

    def next_page(self):
        """Go to next page."""
        total_pages = max(1, (len(self.filtered_files) + self.page_size - 1) // self.page_size)
        if self.current_page < total_pages:
            self.current_page += 1
            self.update_table()
