"""
Application launcher for the S3 downloader GUI.
"""

import logging
import sys

from PySide6.QtCore import QCoreApplication
from PySide6.QtWidgets import QApplication

from .main_window import MainWindow

logging.basicConfig(
    level=logging.INFO, format="%(asctime)s - %(name)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger(__name__)


def run_app():
    """Run the S3 downloader GUI application."""
    # set application info
    QCoreApplication.setOrganizationName("MMI")
    QCoreApplication.setApplicationName("S3 Downloader")

    # create application
    app = QApplication(sys.argv)

    # set application style
    app.setStyle("Fusion")

    # create and show main window
    main_window = MainWindow()
    main_window.show()

    logger.info("Application started")

    # run application
    return app.exec()


if __name__ == "__main__":
    sys.exit(run_app())
