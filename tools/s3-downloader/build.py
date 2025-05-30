#!/usr/bin/env python3
"""
Simplified build script for S3 Downloader using PyInstaller.
"""

import os
import shutil
import subprocess
import sys
from pathlib import Path


def ensure_env_file():
    """Check if .env file exists."""
    env_file = Path(".env")
    if not env_file.exists():
        print("❌ No .env file found. Please create one first.")
        return False
    return True


def clean_build():
    """Clean previous build artifacts with error handling."""
    dirs_to_clean = ["build", "dist", "__pycache__"]

    for dir_name in dirs_to_clean:
        if Path(dir_name).exists():
            try:
                shutil.rmtree(dir_name)
            except PermissionError:
                print(f"⚠️  Cannot remove {dir_name}/ (in use). Skipping...")
                continue


def run_pyinstaller():
    """Run PyInstaller."""
    spec_file = Path("s3_downloader.spec")
    if not spec_file.exists():
        print("❌ s3_downloader.spec file not found!")
        return False

    try:
        cmd = [sys.executable, "-m", "PyInstaller", "--clean", str(spec_file)]
        subprocess.run(cmd, check=True, capture_output=True, text=True)
        return True
    except subprocess.CalledProcessError as e:
        print(f"❌ PyInstaller failed: {e}")
        return False


def main():
    """Main build function."""
    # Change to script directory
    script_dir = Path(__file__).parent
    os.chdir(script_dir)

    # Check .env file
    if not ensure_env_file():
        return 1

    # Clean previous builds
    clean_build()

    # Run PyInstaller
    if not run_pyinstaller():
        return 1

    # Check result
    exe_file = Path("dist/S3Downloader.exe")
    if exe_file.exists():
        print(f"✅ Build complete: {exe_file}")
    else:
        print("❌ Build failed - executable not found")
        return 1

    return 0


if __name__ == "__main__":
    sys.exit(main())
