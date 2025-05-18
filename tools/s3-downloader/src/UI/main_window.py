"""
Main window for the S3 downloader application.
"""

import datetime
import logging
import os
import shutil
from pathlib import Path

from PySide6.QtCore import QTimer, Slot
from PySide6.QtWidgets import (
    QFileDialog,
    QGroupBox,
    QHBoxLayout,
    QLabel,
    QLineEdit,
    QMainWindow,
    QProgressBar,
    QPushButton,
    QTextEdit,
    QVBoxLayout,
    QWidget,
)

from .s3_controller import S3Controller

logging.basicConfig(
    level=logging.INFO, format="%(asctime)s - %(name)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger(__name__)


class MainWindow(QMainWindow):
    """Main window for the S3 downloader application."""

    def __init__(self):
        """Initialize the main window."""
        super().__init__()

        self.setWindowTitle("S3 Downloader")
        self.setMinimumSize(600, 400)

        # initialize S3 controller
        self.s3_controller = S3Controller()

        # initialize timers
        self.timer = QTimer(self)
        self.timer.timeout.connect(self._execute_download_task)

        # disk space update timer (update every 10 seconds)
        self.disk_space_timer = QTimer(self)
        self.disk_space_timer.timeout.connect(self.update_disk_space_info)
        self.disk_space_timer.setInterval(10000)  # 10 seconds

        # next download time update timer (update every second)
        self.time_update_timer = QTimer(self)
        self.time_update_timer.timeout.connect(self.update_next_download_time)
        self.time_update_timer.setInterval(1000)  # 1 second

        # store next download time
        self.next_download_time = None

        # space warning threshold (10%)
        self.space_warning_threshold = 0.1

        # create central widget
        self.central_widget = QWidget()
        self.setCentralWidget(self.central_widget)

        # create main layout
        self.main_layout = QVBoxLayout(self.central_widget)

        # create storage path selection area
        self.create_storage_path_area()

        # create disk space info area
        self.create_disk_space_area()

        # create buttons area
        self.create_buttons_area()

        # create log area
        self.create_log_area()

        # create status bar
        self.statusBar().showMessage("Ready")

        # load settings from env
        self.load_settings_from_env()

        # create schedule info area (after loading settings)
        self.create_schedule_info_area()

        # update disk space info initially
        self.update_disk_space_info()

        # start disk space timer
        self.disk_space_timer.start()

        logger.info("Main window initialized")

    def create_storage_path_area(self):
        """Create storage path selection area."""
        storage_layout = QHBoxLayout()

        # storage path label
        path_label = QLabel("Storage Path:")
        storage_layout.addWidget(path_label)

        # storage path input
        self.storage_path_input = QLineEdit()
        self.storage_path_input.setPlaceholderText("Select local storage path")
        storage_layout.addWidget(
            self.storage_path_input, 1
        )  # 1 is stretch factor, make input box take up more space

        # browse button
        browse_btn = QPushButton("Browse...")
        browse_btn.clicked.connect(self.browse_storage_path)
        storage_layout.addWidget(browse_btn)

        self.main_layout.addLayout(storage_layout)

    def create_disk_space_area(self):
        """Create disk space information area."""
        # Create simple layout for disk space info
        disk_space_layout = QHBoxLayout()

        # Disk space label
        disk_space_label = QLabel("Disk Space:")
        disk_space_layout.addWidget(disk_space_label)

        # Create progress bar for disk space
        self.disk_space_bar = QProgressBar()
        self.disk_space_bar.setRange(0, 100)
        self.disk_space_bar.setValue(0)
        self.disk_space_bar.setTextVisible(True)
        self.disk_space_bar.setFormat("%p% used")
        disk_space_layout.addWidget(self.disk_space_bar, 1)  # 1 is stretch factor

        # Free space label (simplified)
        self.free_space_label = QLabel("Free: N/A")
        disk_space_layout.addWidget(self.free_space_label)

        # For compatibility with existing code, create empty labels
        self.total_space_label = QLabel()
        self.total_space_label.setVisible(False)
        self.used_space_label = QLabel()
        self.used_space_label.setVisible(False)

        # Warning label (hidden by default)
        self.space_warning_label = QLabel()
        self.space_warning_label.setStyleSheet("color: red; font-weight: bold;")
        self.space_warning_label.setVisible(False)
        disk_space_layout.addWidget(self.space_warning_label)

        self.main_layout.addLayout(disk_space_layout)

    def create_buttons_area(self):
        """Create buttons area."""
        buttons_layout = QHBoxLayout()

        # test connection button
        self.test_connection_btn = QPushButton("Test Connection")
        self.test_connection_btn.setMinimumHeight(40)
        self.test_connection_btn.setStyleSheet("font-size: 12px; font-weight: bold;")
        self.test_connection_btn.clicked.connect(self.test_connection)
        buttons_layout.addWidget(self.test_connection_btn)

        # download button
        self.download_btn = QPushButton("Start Download")
        self.download_btn.setMinimumHeight(40)
        self.download_btn.setStyleSheet("font-size: 12px; font-weight: bold;")
        self.download_btn.clicked.connect(self.toggle_download)
        buttons_layout.addWidget(self.download_btn)

        self.main_layout.addLayout(buttons_layout)

    def create_schedule_info_area(self):
        """Create schedule information area."""
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
        self.interval_label = QLabel(f"Download interval: {self.scheduler_interval} seconds")
        schedule_layout.addWidget(self.interval_label)

        schedule_group.setLayout(schedule_layout)
        self.main_layout.addWidget(schedule_group)

    def update_next_download_time(self):
        """Update the next download time display."""
        if self.next_download_time and self.timer.isActive():
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

    def load_settings_from_env(self):
        """Load settings from environment variables."""
        from dotenv import load_dotenv

        load_dotenv()

        self.endpoint_url = os.getenv(
            "S3_SERVICE_URL", "https://gcpe-mmi-storage.objectstore.gov.bc.ca"
        )
        self.bucket_name = os.getenv("S3_BUCKET_NAME", "local")
        self.access_key = os.getenv("S3_ACCESS_KEY", "")
        self.secret_key = os.getenv("S3_SECRET_KEY", "")
        self.local_path = os.getenv("LOCAL_STORAGE_PATH", "./downloads")
        self.timeout = int(os.getenv("S3_TIMEOUT", "2"))
        self.scheduler_interval = int(
            os.getenv("SCHEDULER_INTERVAL", "3600")
        )  # Default to 3600 seconds (1 hour)

        # set storage path input
        self.storage_path_input.setText(self.local_path)

        # set timer interval
        self.timer.setInterval(self.scheduler_interval * 1000)  # interval is in milliseconds

    @Slot()
    def browse_storage_path(self):
        """Open file dialog to select storage path."""
        directory = QFileDialog.getExistingDirectory(
            self, "Select Storage Directory", self.storage_path_input.text() or str(Path.home())
        )
        if directory:
            self.storage_path_input.setText(directory)
            self.local_path = directory
            self.log_message(f"Selected storage path: {directory}")

            # Update disk space information for the new path
            self.update_disk_space_info()

    def update_disk_space_info(self):
        """Update disk space information for the current storage path."""
        storage_path = self.storage_path_input.text().strip()

        if not storage_path:
            # No path selected, clear disk space info
            self.disk_space_bar.setValue(0)
            self.free_space_label.setText("Free: N/A")
            self.space_warning_label.setVisible(False)
            return

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
            if free_ratio < self.space_warning_threshold:
                # Show warning message
                self.space_warning_label.setText("Low space!")
                self.space_warning_label.setVisible(True)
            else:
                self.space_warning_label.setVisible(False)

        except Exception as e:
            self.log_message(f"Error getting disk space info: {e}")
            # Clear disk space info on error
            self.disk_space_bar.setValue(0)
            self.free_space_label.setText("Free: Error")
            self.space_warning_label.setVisible(False)

    def create_log_area(self):
        """Create log area."""
        log_group = QGroupBox("Log")
        log_layout = QVBoxLayout()

        # log area
        self.log_text = QTextEdit()
        self.log_text.setReadOnly(True)
        log_layout.addWidget(self.log_text)

        # progress bar
        self.progress_bar = QProgressBar()
        self.progress_bar.setRange(0, 100)
        self.progress_bar.setValue(0)
        log_layout.addWidget(self.progress_bar)

        # clear log button
        self.clear_log_btn = QPushButton("Clear Log")
        self.clear_log_btn.clicked.connect(self.clear_log)
        log_layout.addWidget(self.clear_log_btn)

        log_group.setLayout(log_layout)
        self.main_layout.addWidget(log_group)

    @Slot()
    def test_connection(self):
        """Test connection to S3."""
        endpoint_url = self.endpoint_url
        bucket_name = self.bucket_name
        access_key = self.access_key
        secret_key = self.secret_key
        timeout = self.timeout
        local_path = self.local_path

        # validate required fields
        if not all([endpoint_url, bucket_name, access_key, secret_key]):
            self.log_message("Error: Please set S3 connection parameters in .env file")
            self.statusBar().showMessage("Error: Missing S3 connection parameters")
            return

        self.log_message(f"Testing connection to {bucket_name} with {timeout}s timeout...")
        self.statusBar().showMessage("Testing connection...")

        # disable test button
        self.test_connection_btn.setEnabled(False)

        # initialize S3 client
        if not self.s3_controller.initialize_client(
            bucket_name=bucket_name,
            endpoint_url=endpoint_url,
            access_key=access_key,
            secret_key=secret_key,
            local_storage_path=local_path,
            timeout=timeout,
        ):
            self.log_message("Error: Failed to initialize S3 client")
            self.statusBar().showMessage("Error: Failed to initialize S3 client")
            self.test_connection_btn.setEnabled(True)
            return

        # test connection
        self.s3_controller.test_connection(self.on_connection_test_finished)

    def on_connection_test_finished(self, success: bool, message: str, data=None):
        """Handle connection test result."""
        self.test_connection_btn.setEnabled(True)

        if success:
            self.log_message("Connection successful!")
            self.statusBar().showMessage("Connection successful")
        else:
            self.log_message(f"Error: Connection failed: {message}")
            self.statusBar().showMessage("Connection failed")

    @Slot()
    def toggle_download(self):
        """Toggle periodic download."""
        if self.timer.isActive():
            # Stop the download timer
            self.timer.stop()

            # Stop the time update timer
            self.time_update_timer.stop()

            # Update UI
            self.download_btn.setText("Start Download")
            self.statusBar().showMessage("Periodic download stopped")
            self.log_message("Periodic download stopped.")

            # Update schedule status
            self.schedule_status_label.setText("Automatic download is not running")
            self.next_download_label.setText("Next download: Not scheduled")
            self.next_download_time = None
        else:
            # Before starting, ensure S3 client is initialized and path is set
            if not hasattr(self.s3_controller, "s3_client") or self.s3_controller.s3_client is None:
                self.log_message("Error: S3 client not initialized. Please test connection first.")
                self.statusBar().showMessage("Error: Please test connection first")
                return

            storage_path = self.storage_path_input.text().strip()
            if not storage_path:
                self.log_message("Error: Storage path not set. Please select a storage path.")
                self.statusBar().showMessage("Error: Please select a storage path")
                return

            # ensure storage path exists
            Path(storage_path).mkdir(parents=True, exist_ok=True)

            # Start the download timer
            self.timer.start()

            # Update UI
            self.download_btn.setText("Stop Download")
            self.statusBar().showMessage(
                f"Periodic download started ({self.scheduler_interval}s interval)"
            )
            self.log_message(f"Periodic download started with {self.scheduler_interval}s interval.")

            # Update schedule status
            self.schedule_status_label.setText("Automatic download is running")
            self.interval_label.setText(f"Download interval: {self.scheduler_interval} seconds")

            # Set next download time
            self.next_download_time = datetime.datetime.now() + datetime.timedelta(
                seconds=self.scheduler_interval
            )

            # Start the time update timer
            self.time_update_timer.start()

            # Update the next download time display
            self.update_next_download_time()

            # Immediately execute the first download task
            self._execute_download_task()

    def _execute_download_task(self):
        """Execute the S3 download task."""
        # get storage path
        storage_path = self.storage_path_input.text().strip()
        if not storage_path:
            # This should not happen if toggle_download is called first, but as a safeguard
            self.log_message("Error: Storage path not set for download task.")
            self.statusBar().showMessage("Error: Storage path not set.")
            return

        # Update disk space info before downloading
        self.update_disk_space_info()

        # Simple check if there's enough disk space
        try:
            _, _, free = shutil.disk_usage(storage_path)
            free_ratio = free / shutil.disk_usage(storage_path)[0]

            if free_ratio < self.space_warning_threshold:
                # Not enough space, show warning and pause downloads
                self.log_message(f"Warning: Low disk space! Only {free_ratio:.1%} available.")

                # If timer is active, stop it
                if self.timer.isActive():
                    self.timer.stop()
                    self.time_update_timer.stop()
                    self.download_btn.setText("Start Download")
                    self.log_message("Download stopped due to low disk space.")

                    # Update schedule status
                    self.schedule_status_label.setText(
                        "Automatic download stopped (low disk space)"
                    )
                    self.next_download_label.setText("Next download: Not scheduled")
                    self.next_download_time = None

                # Show warning in log and status bar
                free_gb = free / 1_000_000_000
                self.log_message(
                    f"Warning: Download paused due to low disk space. Free: {free_gb:.1f} GB"
                )
                self.statusBar().showMessage(
                    f"Download paused: Low disk space ({free_gb:.1f} GB free)"
                )
                return
        except Exception as e:
            self.log_message(f"Error checking disk space: {e}")
            # Continue with download attempt

        self.log_message(f"Downloading to: {storage_path}")
        self.statusBar().showMessage("Downloading...")

        # reset progress bar
        self.progress_bar.setValue(0)

        # call S3 controller to download files
        self.s3_controller.download_files(
            prefix="",  # download all files
            local_dir=storage_path,
            on_progress=self.on_download_progress,
            on_finished=self.on_download_finished,
        )

    def on_download_progress(self, progress: int, message: str):
        """Handle download progress updates."""
        self.progress_bar.setValue(progress)
        self.log_message(message)
        self.statusBar().showMessage(message)

    def on_download_finished(self, success: bool, message: str, data=None):
        """Handle download completion."""
        # Button state is managed by toggle_download, not here.

        if success:
            self.log_message(message)

            # if there is detailed data, show concise stats
            if data and isinstance(data, dict) and data.get("failed", 0) > 0:
                total = data.get("total", 0)
                successful = data.get("successful", 0)
                failed = data.get("failed", 0)
                self.log_message(f"Stats: {successful} ok, {failed} failed, {total} total")

            self.statusBar().showMessage(message)

            # Update disk space info after download
            self.update_disk_space_info()

            # If timer is active, update next download time
            if self.timer.isActive():
                # Set next download time
                self.next_download_time = datetime.datetime.now() + datetime.timedelta(
                    seconds=self.scheduler_interval
                )
                # Update the display
                self.update_next_download_time()
                # Update status message
                self.schedule_status_label.setText("Automatic download is running")

            # Update status bar with success message
            self.statusBar().showMessage(f"Download complete: {message}")
        else:
            self.log_message(f"Error: {message}")
            self.statusBar().showMessage(f"Download failed: {message}")

            # If timer is active, still update next download time despite failure
            if self.timer.isActive():
                self.next_download_time = datetime.datetime.now() + datetime.timedelta(
                    seconds=self.scheduler_interval
                )
                self.update_next_download_time()
                self.schedule_status_label.setText(
                    "Automatic download is running (last attempt failed)"
                )

    @Slot()
    def clear_log(self):
        """Clear the log text area."""
        self.log_text.clear()
        self.progress_bar.setValue(0)
        self.statusBar().showMessage("Log cleared")

    def log_message(self, message: str):
        """Add a message to the log text area."""
        self.log_text.append(message)
        # auto scroll to bottom
        self.log_text.verticalScrollBar().setValue(self.log_text.verticalScrollBar().maximum())
        logger.info(message)
