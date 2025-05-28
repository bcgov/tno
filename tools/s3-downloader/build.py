#!/usr/bin/env python3
"""
Build script for S3 Downloader using PyInstaller.
This script handles the complete build process including .env file integration.
"""

import os
import shutil
import subprocess
import sys
from pathlib import Path


def ensure_env_file():
    """Ensure .env file exists with real configuration for embedding."""
    env_file = Path(".env")
    env_example = Path("env_example.txt")

    if not env_file.exists():
        print("⚠️  No .env file found. Let's create one with your configuration...")
        print("📝 This configuration will be embedded into the exe file.")
        print("🔒 The user won't need to configure anything - it will work out of the box!")
        print()

        if not env_example.exists():
            print("❌ env_example.txt file not found!")
            return False

        # Interactive configuration
        print("Please provide your S3 configuration:")
        print("(Press Enter for default values shown in brackets)")
        print()

        # S3 Configuration
        print("=== S3 Configuration ===")
        s3_url = input("S3 Service URL [https://your-s3-endpoint.com]: ").strip()
        if not s3_url:
            s3_url = "https://your-s3-endpoint.com"

        bucket_name = input("S3 Bucket Name [your-bucket-name]: ").strip()
        if not bucket_name:
            bucket_name = "your-bucket-name"

        access_key = input("S3 Access Key [your-access-key]: ").strip()
        if not access_key:
            access_key = "your-access-key"

        secret_key = input("S3 Secret Key [your-secret-key]: ").strip()
        if not secret_key:
            secret_key = "your-secret-key"

        timeout = input("S3 Timeout in seconds [2]: ").strip()
        if not timeout:
            timeout = "2"

        print("\n=== Storage Configuration ===")
        storage_path = input("Local Storage Path [./downloads]: ").strip()
        if not storage_path:
            storage_path = "./downloads"

        print("\n=== Advanced Configuration (press Enter for defaults) ===")
        scheduler_interval = input("Download Interval in seconds [3600]: ").strip()
        if not scheduler_interval:
            scheduler_interval = "3600"

        max_failures = input("Max Consecutive Failures [3]: ").strip()
        if not max_failures:
            max_failures = "3"

        failure_percentage = input("Max Failure Percentage [0.3]: ").strip()
        if not failure_percentage:
            failure_percentage = "0.3"

        test_interval = input("Network Test Interval [5]: ").strip()
        if not test_interval:
            test_interval = "5"

        warning_threshold = input("Disk Space Warning Threshold [0.1]: ").strip()
        if not warning_threshold:
            warning_threshold = "0.1"

        # Create .env content
        env_content = f"""# S3 Configuration (Embedded in EXE)
S3_SERVICE_URL={s3_url}
S3_BUCKET_NAME={bucket_name}
S3_ACCESS_KEY={access_key}
S3_SECRET_KEY={secret_key}
S3_TIMEOUT={timeout}

# Local Storage Configuration
LOCAL_STORAGE_PATH={storage_path}

# Database Configuration
DATABASE_URL=sqlite:///./s3_downloader.db

# Scheduler Configuration
SCHEDULER_INTERVAL={scheduler_interval}

# Error Handling Configuration
MAX_CONSECUTIVE_FAILURES={max_failures}
MAX_FAILURE_PERCENTAGE={failure_percentage}
NETWORK_TEST_INTERVAL={test_interval}

# Disk Space Configuration
SPACE_WARNING_THRESHOLD={warning_threshold}
"""

        # Write .env file
        with open(env_file, "w") as f:
            f.write(env_content)

        print("\n✅ Created .env file with your configuration.")
        print("🔒 This configuration will be embedded in the exe file.")
        print("👤 Users won't need to configure anything!")

    else:
        print("✅ Found existing .env file.")
        print("🔒 This configuration will be embedded in the exe file.")

    return True


def clean_build():
    """Clean previous build artifacts."""
    print("🧹 Cleaning previous build artifacts...")

    # Directories to clean
    dirs_to_clean = ["build", "dist", "__pycache__"]

    for dir_name in dirs_to_clean:
        if Path(dir_name).exists():
            shutil.rmtree(dir_name)
            print(f"  Removed {dir_name}/")

    # Files to clean
    files_to_clean = ["*.pyc", "*.pyo", "*.spec~"]
    for pattern in files_to_clean:
        for file in Path(".").glob(pattern):
            file.unlink()
            print(f"  Removed {file}")

    print("✅ Cleanup complete.")


def run_pyinstaller():
    """Run PyInstaller with the spec file."""
    print("🔨 Running PyInstaller...")

    spec_file = Path("s3_downloader.spec")
    if not spec_file.exists():
        print("❌ s3_downloader.spec file not found!")
        return False

    try:
        # Run PyInstaller
        cmd = [sys.executable, "-m", "PyInstaller", "--clean", str(spec_file)]
        subprocess.run(cmd, check=True, capture_output=True, text=True)

        print("✅ PyInstaller completed successfully.")
        return True

    except subprocess.CalledProcessError as e:
        print(f"❌ PyInstaller failed with error code {e.returncode}")
        print("STDOUT:", e.stdout)
        print("STDERR:", e.stderr)
        return False


def copy_additional_files():
    """Copy additional files to the dist directory."""
    print("📋 Copying additional files...")

    dist_dir = Path("dist")
    if not dist_dir.exists():
        print("❌ dist directory not found!")
        return False

    # Files to copy
    files_to_copy = ["README.md", "env_example.txt"]

    for file_name in files_to_copy:
        src_file = Path(file_name)
        if src_file.exists():
            dst_file = dist_dir / file_name
            shutil.copy2(src_file, dst_file)
            print(f"  Copied {file_name}")

    print("✅ Additional files copied.")
    return True


def print_summary():
    """Print build summary."""
    dist_dir = Path("dist")
    exe_file = dist_dir / "S3Downloader.exe"

    print("\n" + "=" * 50)
    print("🎉 BUILD COMPLETE!")
    print("=" * 50)

    if exe_file.exists():
        file_size = exe_file.stat().st_size / (1024 * 1024)  # Size in MB
        print(f"📦 Executable: {exe_file}")
        print(f"📏 Size: {file_size:.1f} MB")

    print(f"📁 Output directory: {dist_dir.absolute()}")
    print("\n📋 Files in dist directory:")

    if dist_dir.exists():
        for item in sorted(dist_dir.iterdir()):
            if item.is_file():
                size = item.stat().st_size
                if size > 1024 * 1024:
                    size_str = f"{size / (1024 * 1024):.1f} MB"
                elif size > 1024:
                    size_str = f"{size / 1024:.1f} KB"
                else:
                    size_str = f"{size} B"
                print(f"  📄 {item.name} ({size_str})")

    print("\n🚀 To run the application:")
    print("  1. Navigate to the dist folder")
    print("  2. Run S3Downloader.exe")
    print("  3. No configuration needed - everything is embedded!")
    print("\n🔒 Configuration is embedded in the exe file.")
    print("👤 Users can run it directly without any setup!")


def main():
    """Main build function."""
    print("🔨 S3 Downloader Build Script")
    print("=" * 40)

    # Change to script directory
    script_dir = Path(__file__).parent
    os.chdir(script_dir)
    print(f"📁 Working directory: {Path.cwd()}")

    # Step 1: Ensure .env file with embedded configuration
    if not ensure_env_file():
        print("\n❌ Build cancelled. Failed to create configuration.")
        return 1

    # Step 2: Clean previous builds
    clean_build()

    # Step 3: Run PyInstaller
    if not run_pyinstaller():
        print("\n❌ Build failed during PyInstaller execution.")
        return 1

    # Step 4: Copy additional files
    if not copy_additional_files():
        print("\n⚠️  Warning: Could not copy additional files.")

    # Step 5: Print summary
    print_summary()

    return 0


if __name__ == "__main__":
    sys.exit(main())
