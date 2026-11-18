"""
Application launcher for the S3 downloader GUI.
"""

import atexit
import logging
import sys

from PySide6.QtCore import QCoreApplication
from PySide6.QtWidgets import QApplication

from ..database import close_database, initialize_database

# Import settings to ensure they are loaded before database initialization
# pylint: disable=unused-import
from ..settings import settings  # noqa
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

    # Initialize database
    initialize_database()

    # Register database close function to be called on exit
    atexit.register(close_database)

    # create application
    app = QApplication(sys.argv)

    # set application style
    app.setStyle("Fusion")

    # create and show main window
    main_window = MainWindow()
    main_window.show()

    logger.info("Application started with database initialized")

    # run application
    return app.exec()


if __name__ == "__main__":
    sys.exit(run_app())
