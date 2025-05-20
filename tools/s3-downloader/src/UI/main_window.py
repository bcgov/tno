"""
Main window for the S3 downloader application.
"""

import logging

from PySide6.QtCore import Slot
from PySide6.QtWidgets import QHBoxLayout, QMainWindow, QSplitter, QVBoxLayout, QWidget

from .components.buttons_panel import ButtonsPanel
from .components.disk_space_widget import DiskSpaceWidget
from .components.history_widget import HistoryWidget
from .components.log_widget import LogWidget
from .components.schedule_info_widget import ScheduleInfoWidget
from .components.storage_path_widget import StoragePathWidget
from .controllers.disk_monitor import DiskMonitor
from .controllers.download_controller import DownloadController
from .controllers.scheduler import Scheduler
from .controllers.settings_controller import SettingsController

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
        self.setMinimumSize(1400, 600)  # Increased size to accommodate history panel

        # Initialize controllers
        self.settings_controller = SettingsController()
        self.download_controller = DownloadController(self.settings_controller)
        self.disk_monitor = DiskMonitor(self.settings_controller.space_warning_threshold)
        self.scheduler = Scheduler(self.settings_controller.scheduler_interval)

        # Create central widget and main layout
        self.central_widget = QWidget()
        self.setCentralWidget(self.central_widget)

        # Create horizontal layout for main window
        self.main_layout = QHBoxLayout(self.central_widget)

        # Create splitter for resizable panels
        self.splitter = QSplitter()
        self.main_layout.addWidget(self.splitter)

        # Create left panel (download controls)
        self.left_panel = QWidget()
        self.left_layout = QVBoxLayout(self.left_panel)

        # Create right panel (history)
        self.right_panel = QWidget()
        self.right_layout = QVBoxLayout(self.right_panel)

        # Add panels to splitter
        self.splitter.addWidget(self.left_panel)
        self.splitter.addWidget(self.right_panel)

        # Set initial sizes (left, right ratio)
        self.splitter.setSizes([400, 600])

        # Create UI components
        self.create_ui_components()

        # Connect signals and slots
        self.connect_signals()

        # Initialize UI with settings
        self.initialize_ui()

        # Create status bar
        self.statusBar().showMessage("Ready")

        logger.info("Main window initialized")

    def create_ui_components(self):
        """Create UI components."""
        # Left panel components (download controls)

        # Storage path widget
        self.storage_path_widget = StoragePathWidget()
        self.left_layout.addWidget(self.storage_path_widget)

        # Disk space widget
        self.disk_space_widget = DiskSpaceWidget(
            warning_threshold=self.settings_controller.space_warning_threshold
        )
        self.left_layout.addWidget(self.disk_space_widget)

        # Buttons panel
        self.buttons_panel = ButtonsPanel()
        self.left_layout.addWidget(self.buttons_panel)

        # Schedule info widget
        self.schedule_info_widget = ScheduleInfoWidget(
            interval=self.settings_controller.scheduler_interval
        )
        self.left_layout.addWidget(self.schedule_info_widget)

        # Log widget
        self.log_widget = LogWidget()
        self.left_layout.addWidget(self.log_widget)

        # Right panel components (history)

        # History widget
        self.history_widget = HistoryWidget(s3_controller=self.download_controller.s3_controller)
        self.right_layout.addWidget(self.history_widget)

    def connect_signals(self):
        """Connect signals and slots."""
        # Storage path widget signals
        self.storage_path_widget.path_changed.connect(self.on_storage_path_changed)

        # Buttons panel signals
        self.buttons_panel.test_connection_clicked.connect(self.download_controller.test_connection)
        self.buttons_panel.download_clicked.connect(self.on_download_button_clicked)

        # Download controller signals
        self.download_controller.download_started.connect(self.on_download_started)
        self.download_controller.download_stopped.connect(self.on_download_stopped)
        self.download_controller.download_progress.connect(self.on_download_progress)
        self.download_controller.download_finished.connect(self.on_download_finished)
        self.download_controller.connection_test_started.connect(self.on_connection_test_started)
        self.download_controller.connection_test_finished.connect(self.on_connection_test_finished)

        # Disk monitor signals
        self.disk_monitor.space_status_changed.connect(self.on_disk_space_status_changed)

        # Scheduler signals
        self.scheduler.download_triggered.connect(self.on_scheduled_download)
        self.scheduler.time_updated.connect(self.on_scheduler_time_updated)

    def initialize_ui(self):
        """Initialize UI with settings."""
        # Set storage path
        self.storage_path_widget.set_storage_path(self.settings_controller.local_path)

        # Update disk space info
        self.disk_space_widget.update_disk_space_info(self.settings_controller.local_path)

        # Start disk monitoring
        self.disk_monitor.start_monitoring(self.settings_controller.local_path)

        # Load initial history data
        self.refresh_history()

    @Slot(str)
    def on_storage_path_changed(self, path):
        """
        Handle storage path change.

        Args:
            path: New storage path
        """
        self.settings_controller.update_local_path(path)
        self.disk_monitor.start_monitoring(path)
        self.disk_space_widget.update_disk_space_info(path)
        self.log_widget.log_message(f"Storage path set to: {path}")

    @Slot()
    def on_download_button_clicked(self):
        """Handle download button click."""
        self.download_controller.toggle_download()

    @Slot()
    def on_download_started(self):
        """Handle download start."""
        self.buttons_panel.set_download_active(True)
        self.log_widget.log_message("Download started")
        self.statusBar().showMessage("Download started")

        # Start scheduler
        self.scheduler.start(
            run_immediately=False
        )  # Don't run immediately, we're already starting a download
        self.schedule_info_widget.set_schedule_active(True)
        self.schedule_info_widget.set_next_download_time(self.scheduler.get_next_download_time())

    @Slot()
    def on_download_stopped(self):
        """Handle download stop."""
        self.buttons_panel.set_download_active(False)
        self.log_widget.log_message("Download stopped")
        self.statusBar().showMessage("Download stopped")

        # Stop scheduler
        self.scheduler.stop()
        self.schedule_info_widget.set_schedule_active(False)

    @Slot(int, str)
    def on_download_progress(self, progress, message):
        """
        Handle download progress.

        Args:
            progress: Progress percentage (0-100)
            message: Progress message
        """
        self.log_widget.set_progress(progress)
        self.log_widget.log_message(message)
        self.statusBar().showMessage(message)

    @Slot(bool, str, object)
    def on_download_finished(self, success, message, data):
        """
        Handle download completion.

        Args:
            success: Whether the download was successful
            message: Result message
            data: Additional data (if any)
        """
        self.log_widget.log_message(message)

        # If there is detailed data, show concise stats
        if data and isinstance(data, dict):
            total = data.get("total", 0)
            successful = data.get("successful", 0)
            failed = data.get("failed", 0)
            task_id = data.get("task_id")

            stats_msg = f"Stats: {successful} ok, {failed} failed, {total} total"
            if task_id:
                stats_msg += f" (Task ID: {task_id})"

            self.log_widget.log_message(stats_msg)

        # Update status bar
        self.statusBar().showMessage(f"Download {'complete' if success else 'failed'}: {message}")

        # Update disk space info
        self.disk_space_widget.update_disk_space_info(self.settings_controller.local_path)

        # Update scheduler info if still active
        if self.scheduler.is_running():
            self.schedule_info_widget.set_schedule_active(True, not success)
            self.schedule_info_widget.set_next_download_time(
                self.scheduler.get_next_download_time()
            )

        # Refresh history panel
        self.history_widget.load_history()

    @Slot()
    def on_connection_test_started(self):
        """Handle connection test start."""
        self.buttons_panel.set_test_connection_enabled(False)
        self.log_widget.log_message("Testing connection...")
        self.statusBar().showMessage("Testing connection...")

    @Slot(bool, str)
    def on_connection_test_finished(self, success, message):
        """
        Handle connection test completion.

        Args:
            success: Whether the test was successful
            message: Result message
        """
        self.buttons_panel.set_test_connection_enabled(True)
        self.log_widget.log_message(
            f"Connection test {'successful' if success else 'failed'}: {message}"
        )
        self.statusBar().showMessage(f"Connection test {'successful' if success else 'failed'}")

    @Slot(bool, float, str)
    def on_disk_space_status_changed(self, is_ok, free_ratio, message):
        """
        Handle disk space status change.

        Args:
            is_ok: Whether disk space is sufficient
            free_ratio: Free space ratio (0.0-1.0)
            message: Status message
        """
        if not is_ok:
            self.log_widget.log_message(f"Warning: Low disk space! Only {free_ratio:.1%} available")
            self.log_widget.log_message(f"Warning: {message}")

            # If download is active, stop it
            if self.download_controller.is_downloading:
                self.download_controller.toggle_download()
                self.schedule_info_widget.set_schedule_paused("low disk space")
                self.log_widget.log_message("Download stopped due to low disk space")

    @Slot()
    def on_scheduled_download(self):
        """Handle scheduled download trigger."""
        self.log_widget.log_message("Scheduled download triggered")
        self.download_controller.execute_download_task()

    @Slot()
    def on_scheduler_time_updated(self):
        """Handle scheduler time update."""
        self.schedule_info_widget.update_next_download_time()

    @Slot()
    def refresh_history(self):
        """Refresh the history panel."""
        self.history_widget.load_history()
