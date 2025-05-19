"""
Main window for the S3 downloader application.
"""

import logging

from PySide6.QtCore import Slot
from PySide6.QtWidgets import QHBoxLayout, QMainWindow, QSplitter, QVBoxLayout, QWidget

from .components.buttons_panel import ButtonsPanel
from .components.history_widget import HistoryWidget
from .components.log_widget import LogWidget
from .components.schedule_info_widget import ScheduleInfoWidget
from .components.storage_info_widget import StorageInfoWidget

# Disk monitoring is now handled by StorageInfoWidget
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
        self.splitter.setSizes([350, 650])  # Adjusted for more history panel space

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

        # Storage info widget (combines path selection and disk space info)
        self.storage_info_widget = StorageInfoWidget(
            warning_threshold=self.settings_controller.space_warning_threshold
        )
        self.left_layout.addWidget(self.storage_info_widget)

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
        # Storage info widget signals
        self.storage_info_widget.path_changed.connect(self.on_storage_path_changed)
        self.storage_info_widget.low_space_warning.connect(self.on_disk_space_status_changed)

        # Buttons panel signals
        self.buttons_panel.download_clicked.connect(self.on_download_button_clicked)

        # Download controller signals
        self.download_controller.download_started.connect(self.on_download_started)
        self.download_controller.download_stopped.connect(self.on_download_stopped)
        self.download_controller.download_progress.connect(self.on_download_progress)
        self.download_controller.download_finished.connect(self.on_download_finished)
        self.download_controller.batch_completed.connect(self.on_batch_completed)
        # Connection testing is now handled automatically during download

        # Disk space monitoring is now handled by storage_info_widget

        # Scheduler signals
        self.scheduler.download_triggered.connect(self.on_scheduled_download)
        self.scheduler.time_updated.connect(self.on_scheduler_time_updated)
        self.scheduler.download_skipped.connect(self.on_scheduled_download_skipped)

    def initialize_ui(self):
        """Initialize UI with settings."""
        # Set storage path and update disk space info
        self.storage_info_widget.set_storage_path(self.settings_controller.local_path)

        # Disk monitoring is handled by storage_info_widget

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
        # Disk space info is updated automatically by storage_info_widget
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

        # Refresh history panel to show the new task that was just created
        # Note: There might be a small delay before the task appears in the database
        # but this ensures the UI is updated as soon as possible
        self.history_widget.load_history()

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

            # Check if this is part of continuous download mode
            if (
                hasattr(self.download_controller, "is_continuous_mode")
                and self.download_controller.is_continuous_mode
            ):
                batch_num = getattr(self.download_controller, "current_batch", 0)
                stats_msg = (
                    f"Batch {batch_num} Stats: {successful} ok, {failed} failed, {total} total"
                )
            else:
                stats_msg = f"Stats: {successful} ok, {failed} failed, {total} total"

            if task_id:
                stats_msg += f" (Task ID: {task_id})"

            self.log_widget.log_message(stats_msg)

        # Only update final status if not in continuous mode or if continuous mode is completed
        is_continuous_active = (
            hasattr(self.download_controller, "is_continuous_mode")
            and self.download_controller.is_continuous_mode
        )

        # Refresh history panel for final completion (batch refreshes are handled by on_batch_completed)
        if not is_continuous_active and data and data.get("task_id"):
            self.history_widget.load_history()

        if not is_continuous_active:
            # Update status bar
            self.statusBar().showMessage(
                f"Download {'complete' if success else 'failed'}: {message}"
            )

            # Update disk space info
            self.storage_info_widget.update_disk_space_info()

            # Update scheduler info if still active
            if self.scheduler.is_running():
                self.schedule_info_widget.set_schedule_active(True, not success)
                self.schedule_info_widget.set_next_download_time(
                    self.scheduler.get_next_download_time()
                )
        else:
            # In continuous mode, just update status to show we're continuing
            self.statusBar().showMessage("Continuous download in progress...")

    @Slot(bool, float)
    def on_disk_space_status_changed(self, is_warning_active, free_ratio):
        """
        Handle disk space status change.

        Args:
            is_warning_active: True if the low disk space warning is active.
            free_ratio: Free space ratio (0.0-1.0)
        """
        if is_warning_active:
            message = f"Low disk space! Only {free_ratio:.1%} available. Please free up some space."
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
        logger.info("Attempting to start scheduled download")

        # Try to execute scheduled download
        success = self.download_controller.execute_scheduled_download()

        if not success:
            # Log skip using scheduler
            self.scheduler.log_skip("download already in progress")

            # Update the next download time for the scheduler
            if self.scheduler.is_running():
                self.schedule_info_widget.set_next_download_time(
                    self.scheduler.get_next_download_time()
                )

    @Slot()
    def on_scheduler_time_updated(self):
        """Handle scheduler time update."""
        self.schedule_info_widget.update_next_download_time()

    @Slot(str)
    def on_scheduled_download_skipped(self, reason):
        """Handle when a scheduled download is skipped."""
        message = f"Scheduled download skipped: {reason}"
        self.log_widget.log_message(message)
        logger.info(message)

    @Slot()
    def refresh_history(self):
        """Refresh the history panel."""
        self.history_widget.load_history()

    @Slot(object)
    def on_batch_completed(self, data):
        """
        Handle batch completion.

        Args:
            data: Additional data from the completed batch
        """
        # Always refresh history panel when a batch is completed
        # This ensures new task records are visible immediately
        if data and data.get("task_id"):
            self.history_widget.load_history()

            # Log batch completion info
            total = data.get("total", 0)
            successful = data.get("successful", 0)
            failed = data.get("failed", 0)
            task_id = data.get("task_id")

            if hasattr(self.download_controller, "current_batch"):
                batch_num = getattr(self.download_controller, "current_batch", 0)
                stats_msg = f"Batch {batch_num} completed: {successful} ok, {failed} failed, {total} total (Task ID: {task_id})"
                self.log_widget.log_message(stats_msg)
