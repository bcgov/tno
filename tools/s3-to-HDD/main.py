import sys
import os
from PyQt6.QtWidgets import (
    QApplication, QMainWindow, QWidget, QVBoxLayout, QHBoxLayout,
    QTreeView, QTableView, QTextEdit, QSplitter, QLabel, QPushButton,
    QFileDialog, QHeaderView, QStatusBar
)
from PyQt6.QtCore import Qt, QSize
from PyQt6.QtGui import QStandardItemModel, QStandardItem


class S3DownloaderApp(QMainWindow):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("S3 to HDD Downloader")
        self.setMinimumSize(1000, 700)

        # Create central widget and main layout
        central_widget = QWidget()
        self.setCentralWidget(central_widget)
        main_layout = QVBoxLayout(central_widget)

        # Create horizontal splitter for main content
        main_splitter = QSplitter(Qt.Orientation.Horizontal)
        main_layout.addWidget(main_splitter)

        # Left panel - S3 Directory Browser
        left_widget = QWidget()
        left_layout = QVBoxLayout(left_widget)
        left_layout.setContentsMargins(0, 0, 0, 0)

        left_label = QLabel("S3 Directories to Download")
        left_label.setStyleSheet("font-weight: bold; font-size: 14px;")
        left_layout.addWidget(left_label)

        # S3 directory tree
        self.directory_tree = QTreeView()
        self.directory_model = QStandardItemModel()
        self.directory_model.setHorizontalHeaderLabels(["S3 Directories"])
        self.directory_tree.setModel(self.directory_model)
        self.directory_tree.setEditTriggers(QTreeView.EditTrigger.NoEditTriggers)
        self.directory_tree.setSelectionMode(QTreeView.SelectionMode.ExtendedSelection)
        left_layout.addWidget(self.directory_tree)

        # Buttons for S3 operations
        s3_buttons_layout = QHBoxLayout()
        self.connect_button = QPushButton("Connect to S3")
        self.download_button = QPushButton("Download Selected")
        s3_buttons_layout.addWidget(self.connect_button)
        s3_buttons_layout.addWidget(self.download_button)
        left_layout.addLayout(s3_buttons_layout)

        # Right panel - Download History
        right_widget = QWidget()
        right_layout = QVBoxLayout(right_widget)
        right_layout.setContentsMargins(0, 0, 0, 0)

        right_label = QLabel("Download History")
        right_label.setStyleSheet("font-weight: bold; font-size: 14px;")
        right_layout.addWidget(right_label)

        # Download history table
        self.history_table = QTableView()
        self.history_model = QStandardItemModel()
        self.history_model.setHorizontalHeaderLabels(["File", "S3 Path", "Local Path", "Date", "Status"])
        self.history_table.setModel(self.history_model)
        self.history_table.horizontalHeader().setSectionResizeMode(QHeaderView.ResizeMode.Stretch)
        right_layout.addWidget(self.history_table)

        # Add widgets to main splitter
        main_splitter.addWidget(left_widget)
        main_splitter.addWidget(right_widget)
        main_splitter.setSizes([400, 600])  # Set initial sizes

        # Bottom panel - Log display
        log_label = QLabel("Log")
        log_label.setStyleSheet("font-weight: bold; font-size: 14px;")
        main_layout.addWidget(log_label)

        self.log_display = QTextEdit()
        self.log_display.setReadOnly(True)
        self.log_display.setMaximumHeight(150)
        main_layout.addWidget(self.log_display)

        # Status bar
        self.status_bar = QStatusBar()
        self.setStatusBar(self.status_bar)
        self.status_bar.showMessage("Ready")

        # Connect signals
        self.connect_button.clicked.connect(self.connect_to_s3)
        self.download_button.clicked.connect(self.download_selected)

        # Add some sample data (for demonstration)
        self.add_sample_data()

        # Log a startup message
        self.log_message("Application started")

    def connect_to_s3(self):
        # This would be implemented to connect to S3 and populate the directory tree
        self.log_message("Connecting to S3...")
        self.status_bar.showMessage("Connected to S3")

    def download_selected(self):
        # This would be implemented to download selected directories
        selected_indexes = self.directory_tree.selectedIndexes()
        if selected_indexes:
            self.log_message(f"Starting download of {len(selected_indexes)} selected items")
            self.status_bar.showMessage("Download started")
        else:
            self.log_message("No directories selected for download")

    def log_message(self, message):
        """Add a message to the log display"""
        self.log_display.append(message)

    def add_sample_data(self):
        """Add sample data for demonstration purposes"""
        # Sample S3 directories
        bucket_item = QStandardItem("my-s3-bucket")
        folder1 = QStandardItem("folder1")
        folder2 = QStandardItem("folder2")
        subfolder1 = QStandardItem("subfolder1")

        folder1.appendRow(subfolder1)
        bucket_item.appendRow(folder1)
        bucket_item.appendRow(folder2)
        self.directory_model.appendRow(bucket_item)

        # Expand all items
        self.directory_tree.expandAll()

        # Sample download history
        for i in range(5):
            row = [
                QStandardItem(f"file{i}.txt"),
                QStandardItem(f"my-s3-bucket/folder{i%2+1}/file{i}.txt"),
                QStandardItem(f"C:/Downloads/s3-files/file{i}.txt"),
                QStandardItem("2023-06-15 14:30"),
                QStandardItem("Completed")
            ]
            self.history_model.appendRow(row)


def main():
    app = QApplication(sys.argv)
    window = S3DownloaderApp()
    window.show()
    sys.exit(app.exec())


if __name__ == "__main__":
    main()
