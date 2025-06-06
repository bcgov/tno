import sys

if __name__ == "__main__":
    try:
        # Load settings before starting the app
        # pylint: disable=unused-import
        import src.settings  # noqa

        # Then import and run the app
        from src.UI.app import run_app

        sys.exit(run_app())
    except ImportError as e:
        print(f"Error loading GUI: {e}")
        print("Make sure PySide6 is installed: uv add PySide6")
        sys.exit(1)
