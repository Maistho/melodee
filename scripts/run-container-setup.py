#!/usr/bin/env python3
"""
Melodee Container Setup Script

This script prepares the environment for running Melodee in containers.
It will:
1. Detect available container runtime (podman or docker)
2. Offer to install podman if no runtime is found
3. Generate secure random values for secrets
4. Create a .env file with all required configuration
5. Optionally start the containers

Usage:
    python scripts/run-container-setup.py [--start]
"""

import os
import secrets
import shutil
import subprocess
import sys
from pathlib import Path


def print_banner():
    """Print the Melodee setup banner."""
    print("\n" + "=" * 60)
    print("  Melodee Container Setup")
    print("=" * 60 + "\n")


def print_success(message: str):
    """Print a success message in green."""
    print(f"✓ {message}")


def print_error(message: str):
    """Print an error message in red."""
    print(f"✗ {message}", file=sys.stderr)


def print_info(message: str):
    """Print an info message."""
    print(f"  {message}")


def print_warning(message: str):
    """Print a warning message."""
    print(f"⚠ {message}")


def detect_os() -> dict | None:
    """Detect the operating system and return info for package installation."""
    os_info = {"type": None, "id": None, "version": None}
    
    # Check for Linux
    if sys.platform.startswith("linux"):
        os_info["type"] = "linux"
        
        # Try to read /etc/os-release
        os_release = Path("/etc/os-release")
        if os_release.exists():
            content = os_release.read_text()
            for line in content.splitlines():
                if line.startswith("ID="):
                    os_info["id"] = line.split("=", 1)[1].strip().strip('"').lower()
                elif line.startswith("VERSION_ID="):
                    os_info["version"] = line.split("=", 1)[1].strip().strip('"')
        
        return os_info
    
    elif sys.platform == "darwin":
        os_info["type"] = "macos"
        os_info["id"] = "macos"
        return os_info
    
    elif sys.platform == "win32":
        os_info["type"] = "windows"
        os_info["id"] = "windows"
        return os_info
    
    return None


def get_install_commands(os_info: dict) -> list[list[str]] | None:
    """Get the commands to install podman and podman-compose for the detected OS."""
    if os_info is None:
        return None
    
    os_id = os_info.get("id", "")
    os_type = os_info.get("type", "")
    
    # Debian/Ubuntu based
    if os_id in ["debian", "ubuntu", "linuxmint", "pop"]:
        return [
            ["sudo", "apt-get", "update"],
            ["sudo", "apt-get", "install", "-y", "podman", "podman-compose"],
        ]
    
    # Fedora
    elif os_id == "fedora":
        return [
            ["sudo", "dnf", "install", "-y", "podman", "podman-compose"],
        ]
    
    # RHEL/CentOS/Rocky/Alma
    elif os_id in ["rhel", "centos", "rocky", "almalinux"]:
        return [
            ["sudo", "dnf", "install", "-y", "podman", "podman-compose"],
        ]
    
    # Arch Linux
    elif os_id in ["arch", "manjaro", "endeavouros"]:
        return [
            ["sudo", "pacman", "-Sy", "--noconfirm", "podman", "podman-compose"],
        ]
    
    # openSUSE
    elif os_id in ["opensuse-leap", "opensuse-tumbleweed", "sles"]:
        return [
            ["sudo", "zypper", "install", "-y", "podman", "podman-compose"],
        ]
    
    # macOS
    elif os_type == "macos":
        if shutil.which("brew"):
            return [
                ["brew", "install", "podman", "podman-compose"],
                ["podman", "machine", "init"],
                ["podman", "machine", "start"],
            ]
        else:
            return None  # Need Homebrew
    
    return None


def install_podman(os_info: dict) -> bool:
    """Attempt to install podman and podman-compose."""
    commands = get_install_commands(os_info)
    
    if commands is None:
        return False
    
    print_info("Installing podman and podman-compose...")
    print()
    
    for cmd in commands:
        print_info(f"Running: {' '.join(cmd)}")
        try:
            result = subprocess.run(cmd, timeout=300)
            if result.returncode != 0:
                print_error(f"Command failed with exit code {result.returncode}")
                return False
        except subprocess.TimeoutExpired:
            print_error("Installation timed out")
            return False
        except OSError as e:
            print_error(f"Failed to run command: {e}")
            return False
    
    print()
    return True


def offer_install_podman() -> str | None:
    """Offer to install podman if no container runtime is found."""
    os_info = detect_os()
    
    if os_info is None:
        print_error("Could not detect operating system")
        return None
    
    os_type = os_info.get("type", "unknown")
    os_id = os_info.get("id", "unknown")
    
    print_info(f"Detected OS: {os_id} ({os_type})")
    
    # Check if we know how to install on this OS
    commands = get_install_commands(os_info)
    
    if commands is None:
        if os_type == "macos" and not shutil.which("brew"):
            print_error("Homebrew is required to install podman on macOS")
            print_info("Install Homebrew first: https://brew.sh")
        elif os_type == "windows":
            print_error("Automatic installation not supported on Windows")
            print_info("Please install Podman Desktop: https://podman-desktop.io")
            print_info("Or Docker Desktop: https://www.docker.com/products/docker-desktop")
        else:
            print_error(f"Automatic installation not supported for {os_id}")
            print_info("Please install podman or docker manually")
        return None
    
    print()
    print_warning("No container runtime (podman or docker) found!")
    print_info("This script can install podman for you.")
    print()
    
    response = input("  Would you like to install podman now? (y/N): ").strip().lower()
    
    if response != 'y':
        print_info("Installation cancelled")
        return None
    
    print()
    if install_podman(os_info):
        # Verify installation
        if shutil.which("podman"):
            print_success("Podman installed successfully!")
            return "podman"
        else:
            print_error("Installation completed but podman not found in PATH")
            print_info("You may need to restart your terminal or log out and back in")
            return None
    else:
        print_error("Failed to install podman")
        return None


def detect_container_runtime() -> str | None:
    """Detect available container runtime, preferring podman over docker."""
    for runtime in ["podman", "docker"]:
        if shutil.which(runtime):
            try:
                result = subprocess.run(
                    [runtime, "--version"],
                    capture_output=True,
                    text=True,
                    timeout=10
                )
                if result.returncode == 0:
                    version_info = result.stdout.strip().split('\n')[0]
                    print_success(f"Found {runtime}: {version_info}")
                    return runtime
            except (subprocess.TimeoutExpired, OSError):
                continue
    return None


def check_compose_available(runtime: str) -> bool:
    """Check if compose is available for the detected runtime."""
    try:
        result = subprocess.run(
            [runtime, "compose", "version"],
            capture_output=True,
            text=True,
            timeout=10
        )
        if result.returncode == 0:
            version_info = result.stdout.strip().split('\n')[0]
            print_success(f"Found compose: {version_info}")
            return True
    except (subprocess.TimeoutExpired, OSError):
        pass
    
    # Try docker-compose as fallback for docker
    if runtime == "docker" and shutil.which("docker-compose"):
        try:
            result = subprocess.run(
                ["docker-compose", "--version"],
                capture_output=True,
                text=True,
                timeout=10
            )
            if result.returncode == 0:
                print_success(f"Found docker-compose: {result.stdout.strip()}")
                return True
        except (subprocess.TimeoutExpired, OSError):
            pass
    
    return False


def generate_secure_password(length: int = 32) -> str:
    """Generate a secure random password."""
    return secrets.token_urlsafe(length)


def generate_jwt_token() -> str:
    """Generate a secure JWT token (64+ characters)."""
    return secrets.token_urlsafe(64)


def get_project_root() -> Path:
    """Get the project root directory."""
    script_dir = Path(__file__).resolve().parent
    return script_dir.parent


def create_env_file(project_root: Path, overwrite: bool = False) -> bool:
    """Create the .env file with generated secrets."""
    env_file = project_root / ".env"
    example_env = project_root / "example.env"
    
    if env_file.exists() and not overwrite:
        print_info(f".env file already exists at {env_file}")
        response = input("  Overwrite? (y/N): ").strip().lower()
        if response != 'y':
            print_info("Keeping existing .env file")
            return True
    
    # Generate secure values
    db_password = generate_secure_password()
    jwt_token = generate_jwt_token()
    
    env_content = f"""# Melodee Docker Configuration
# Generated by run-container-setup.py
# 
# WARNING: This file contains secrets. Do not commit to version control!

# Database password (auto-generated)
DB_PASSWORD={db_password}

# Database connection pool settings
DB_MIN_POOL_SIZE=10
DB_MAX_POOL_SIZE=50

# Port configuration - the port Melodee will be available on
MELODEE_PORT=8080

# Authentication settings
# JWT token secret (auto-generated, 64+ characters)
MELODEE_AUTH_TOKEN={jwt_token}
# Token validity in hours
MELODEE_AUTH_TOKEN_HOURS=24

# Brave Search API Configuration (optional)
# Get your API key from https://brave.com/search/api/
BRAVE_SEARCH__ENABLED=false
BRAVE_SEARCH__APIKEY=your_brave_api_key_here
BRAVE_SEARCH__BASEURL=https://api.search.brave.com
BRAVE_SEARCH__IMAGESEARCHPATH=/res/v1/images/search
"""
    
    try:
        env_file.write_text(env_content)
        print_success(f"Created .env file at {env_file}")
        return True
    except OSError as e:
        print_error(f"Failed to create .env file: {e}")
        return False


def ensure_gitignore_has_env(project_root: Path):
    """Ensure .env is in .gitignore."""
    gitignore = project_root / ".gitignore"
    
    if not gitignore.exists():
        return
    
    content = gitignore.read_text()
    if ".env" not in content:
        print_info("Adding .env to .gitignore")
        with gitignore.open("a") as f:
            f.write("\n# Environment file with secrets\n.env\n")
        print_success("Updated .gitignore")


def get_compose_command(runtime: str) -> list[str]:
    """Get the appropriate compose command for the runtime."""
    # Check if 'podman compose' (plugin) works
    if runtime == "podman":
        try:
            result = subprocess.run(
                ["podman", "compose", "version"],
                capture_output=True,
                timeout=10
            )
            if result.returncode == 0:
                return ["podman", "compose"]
        except (subprocess.TimeoutExpired, OSError):
            pass
        
        # Fall back to podman-compose (standalone)
        if shutil.which("podman-compose"):
            return ["podman-compose"]
    
    # Docker uses 'docker compose'
    return [runtime, "compose"]


def start_containers(runtime: str, project_root: Path) -> bool:
    """Start the containers using the detected runtime."""
    compose_cmd = get_compose_command(runtime)
    
    # Build the image first (required for podman with local images)
    print_info("Building container image (this may take a while on first run)...")
    try:
        result = subprocess.run(
            [*compose_cmd, "build"],
            cwd=project_root,
            timeout=600  # 10 minute timeout for build
        )
        if result.returncode != 0:
            print_error("Failed to build container image")
            return False
        print_success("Container image built successfully")
    except subprocess.TimeoutExpired:
        print_error("Container build timed out")
        return False
    except OSError as e:
        print_error(f"Failed to build container: {e}")
        return False
    
    # Start the containers
    print_info("Starting containers...")
    try:
        result = subprocess.run(
            [*compose_cmd, "up", "-d"],
            cwd=project_root,
            timeout=120  # 2 minute timeout for start
        )
        return result.returncode == 0
    except subprocess.TimeoutExpired:
        print_error("Container startup timed out")
        return False
    except OSError as e:
        print_error(f"Failed to start containers: {e}")
        return False


def print_next_steps(runtime: str, started: bool):
    """Print next steps for the user."""
    compose_cmd = " ".join(get_compose_command(runtime))
    
    print("\n" + "-" * 60)
    print("  Next Steps")
    print("-" * 60 + "\n")
    
    if started:
        print_info("Containers are starting up!")
        print_info("Once ready, access Melodee at: http://localhost:8080")
        print()
        print_info(f"Useful commands:")
        print(f"    {compose_cmd} logs -f        # View logs")
        print(f"    {compose_cmd} ps             # Check status")
        print(f"    {compose_cmd} down           # Stop containers")
        print(f"    {compose_cmd} build          # Rebuild image")
        print(f"    {compose_cmd} up -d          # Start containers")
    else:
        print_info("To start Melodee, run:")
        print(f"    {compose_cmd} build")
        print(f"    {compose_cmd} up -d")
        print()
        print_info("Then access Melodee at: http://localhost:8080")
    
    print()
    print_info("Default admin credentials are set during first login.")
    print_info("Check the README.md for more information.")
    print()


def main():
    """Main entry point."""
    print_banner()
    
    # Parse arguments
    start_after_setup = "--start" in sys.argv
    
    # Detect container runtime
    print("Detecting container runtime...")
    runtime = detect_container_runtime()
    
    if not runtime:
        # Offer to install podman
        runtime = offer_install_podman()
        
        if not runtime:
            print()
            print_error("Cannot continue without a container runtime.")
            print_info("  - Podman: https://podman.io/getting-started/installation")
            print_info("  - Docker: https://docs.docker.com/get-docker/")
            sys.exit(1)
    
    # Check compose availability
    print("\nChecking compose availability...")
    if not check_compose_available(runtime):
        print_error(f"Compose not available for {runtime}!")
        if runtime == "podman":
            print_info("Install podman-compose: pip install podman-compose")
        else:
            print_info("Install Docker Compose: https://docs.docker.com/compose/install/")
        sys.exit(1)
    
    # Get project root
    project_root = get_project_root()
    print(f"\nProject root: {project_root}")
    
    # Create .env file
    print("\nSetting up environment...")
    if not create_env_file(project_root):
        sys.exit(1)
    
    # Ensure .env is gitignored
    ensure_gitignore_has_env(project_root)
    
    # Optionally start containers
    started = False
    if start_after_setup:
        print("\nStarting containers...")
        started = start_containers(runtime, project_root)
        if started:
            print_success("Containers started successfully!")
        else:
            print_error("Failed to start containers")
    
    # Print next steps
    print_next_steps(runtime, started)
    
    return 0 if (not start_after_setup or started) else 1


if __name__ == "__main__":
    sys.exit(main())
