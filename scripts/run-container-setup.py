#!/usr/bin/env python3
"""
Melodee Container Setup Script

This script prepares the environment for running Melodee in containers.
It will:
1. Detect available container runtime (podman or docker)
2. Offer to install podman if no runtime is found
3. Check system requirements (disk space, memory, ports)
4. Generate secure random values for secrets
5. Create a .env file with all required configuration
6. Verify required files exist (Dockerfile, compose.yml, entrypoint.sh)
7. Optionally start the containers
8. Verify containers are healthy after startup

Usage:
    python scripts/run-container-setup.py [--start] [--check-only] [--force]
    
Options:
    --start       Start containers after setup
    --check-only  Only run checks, don't create .env or start containers
    --force       Overwrite existing .env file without prompting
"""

import os
import secrets
import shutil
import socket
import subprocess
import sys
import time
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


# Minimum requirements
MIN_DISK_SPACE_GB = 5
MIN_MEMORY_GB = 2
REQUIRED_FILES = ["Dockerfile", "compose.yml", "entrypoint.sh"]
DEFAULT_PORT = 8080


def check_disk_space(project_root: Path, min_gb: int = MIN_DISK_SPACE_GB) -> tuple[bool, str]:
    """Check if there's enough disk space for containers."""
    try:
        stat = os.statvfs(project_root)
        free_gb = (stat.f_bavail * stat.f_frsize) / (1024 ** 3)
        if free_gb < min_gb:
            return False, f"Only {free_gb:.1f}GB free, need at least {min_gb}GB"
        return True, f"{free_gb:.1f}GB available"
    except OSError as e:
        return False, f"Could not check disk space: {e}"


def check_memory() -> tuple[bool, str]:
    """Check if system has enough memory."""
    try:
        with open("/proc/meminfo") as f:
            for line in f:
                if line.startswith("MemTotal:"):
                    kb = int(line.split()[1])
                    gb = kb / (1024 ** 2)
                    if gb < MIN_MEMORY_GB:
                        return False, f"Only {gb:.1f}GB RAM, need at least {MIN_MEMORY_GB}GB"
                    return True, f"{gb:.1f}GB RAM available"
    except (OSError, ValueError):
        pass
    # Can't determine on macOS/Windows this way, assume OK
    return True, "Memory check skipped (non-Linux)"


def check_port_available(port: int) -> tuple[bool, str]:
    """Check if a port is available for binding."""
    try:
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.settimeout(1)
            result = s.connect_ex(('localhost', port))
            if result == 0:
                return False, f"Port {port} is already in use"
            return True, f"Port {port} is available"
    except OSError as e:
        return False, f"Could not check port {port}: {e}"


def check_required_files(project_root: Path) -> tuple[bool, list[str]]:
    """Check if all required files exist."""
    missing = []
    for filename in REQUIRED_FILES:
        if not (project_root / filename).exists():
            missing.append(filename)
    return len(missing) == 0, missing


def check_dockerfile_valid(project_root: Path) -> tuple[bool, str]:
    """Basic validation of Dockerfile."""
    dockerfile = project_root / "Dockerfile"
    if not dockerfile.exists():
        return False, "Dockerfile not found"
    
    content = dockerfile.read_text()
    
    # Check for multi-stage build
    if "FROM" not in content:
        return False, "Dockerfile missing FROM instruction"
    
    # Check for entrypoint
    if "ENTRYPOINT" not in content and "CMD" not in content:
        return False, "Dockerfile missing ENTRYPOINT or CMD"
    
    return True, "Dockerfile looks valid"


def check_compose_valid(project_root: Path) -> tuple[bool, str]:
    """Basic validation of compose.yml."""
    compose_file = project_root / "compose.yml"
    if not compose_file.exists():
        return False, "compose.yml not found"
    
    content = compose_file.read_text()
    
    # Check for required services
    if "melodee-db:" not in content:
        return False, "compose.yml missing melodee-db service"
    
    if "melodee.blazor:" not in content:
        return False, "compose.yml missing melodee.blazor service"
    
    # Check for localhost/ prefix (podman compatibility)
    if "image: melodee:latest" in content and "localhost/melodee:latest" not in content:
        return False, "compose.yml should use 'localhost/melodee:latest' for Podman compatibility"
    
    return True, "compose.yml looks valid"


def run_preflight_checks(project_root: Path, port: int = DEFAULT_PORT) -> bool:
    """Run all preflight checks and report results."""
    print("\n" + "-" * 60)
    print("  Preflight Checks")
    print("-" * 60 + "\n")
    
    all_passed = True
    
    # Check required files
    files_ok, missing = check_required_files(project_root)
    if files_ok:
        print_success("All required files present")
    else:
        print_error(f"Missing required files: {', '.join(missing)}")
        all_passed = False
    
    # Check Dockerfile
    dockerfile_ok, dockerfile_msg = check_dockerfile_valid(project_root)
    if dockerfile_ok:
        print_success(dockerfile_msg)
    else:
        print_error(dockerfile_msg)
        all_passed = False
    
    # Check compose.yml
    compose_ok, compose_msg = check_compose_valid(project_root)
    if compose_ok:
        print_success(compose_msg)
    else:
        print_error(compose_msg)
        all_passed = False
    
    # Check disk space
    disk_ok, disk_msg = check_disk_space(project_root)
    if disk_ok:
        print_success(f"Disk space: {disk_msg}")
    else:
        print_error(f"Disk space: {disk_msg}")
        all_passed = False
    
    # Check memory
    mem_ok, mem_msg = check_memory()
    if mem_ok:
        print_success(f"Memory: {mem_msg}")
    else:
        print_warning(f"Memory: {mem_msg}")
        # Don't fail on memory, just warn
    
    # Check port
    port_ok, port_msg = check_port_available(port)
    if port_ok:
        print_success(port_msg)
    else:
        print_warning(port_msg)
        print_info(f"  You can change the port in .env file (MELODEE_PORT)")
    
    print()
    return all_passed


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
            timeout=900  # 15 minute timeout for build
        )
        if result.returncode != 0:
            print_error("Failed to build container image")
            print_info("Check the build output above for errors")
            return False
        print_success("Container image built successfully")
    except subprocess.TimeoutExpired:
        print_error("Container build timed out (15 minutes)")
        print_info("Try running the build manually to see progress")
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
        if result.returncode != 0:
            print_error("Failed to start containers")
            return False
        return True
    except subprocess.TimeoutExpired:
        print_error("Container startup timed out")
        return False
    except OSError as e:
        print_error(f"Failed to start containers: {e}")
        return False


def wait_for_healthy(runtime: str, project_root: Path, timeout: int = 120) -> bool:
    """Wait for containers to become healthy."""
    compose_cmd = get_compose_command(runtime)
    
    print_info(f"Waiting for containers to become healthy (timeout: {timeout}s)...")
    
    start_time = time.time()
    db_healthy = False
    app_healthy = False
    
    while time.time() - start_time < timeout:
        try:
            # Check container status
            result = subprocess.run(
                [*compose_cmd, "ps", "--format", "json"],
                cwd=project_root,
                capture_output=True,
                text=True,
                timeout=10
            )
            
            if result.returncode == 0:
                output = result.stdout.strip()
                
                # Check for health status in output
                if "healthy" in output.lower():
                    # Parse JSON if available, otherwise check text
                    if "melodee-db" in output and "healthy" in output:
                        if not db_healthy:
                            print_success("Database container is healthy")
                            db_healthy = True
                    
                    if "melodee.blazor" in output or "melodee_melodee.blazor" in output:
                        if "healthy" in output:
                            if not app_healthy:
                                print_success("Application container is healthy")
                                app_healthy = True
                
                if db_healthy and app_healthy:
                    return True
            
            # Also try a direct health check
            if not app_healthy:
                try:
                    health_result = subprocess.run(
                        ["curl", "-fsS", "http://localhost:8080/health"],
                        capture_output=True,
                        timeout=5
                    )
                    if health_result.returncode == 0:
                        print_success("Application health check passed")
                        app_healthy = True
                        if db_healthy:
                            return True
                except (subprocess.TimeoutExpired, OSError, FileNotFoundError):
                    pass
            
        except (subprocess.TimeoutExpired, OSError):
            pass
        
        # Show progress
        elapsed = int(time.time() - start_time)
        if elapsed % 10 == 0 and elapsed > 0:
            print_info(f"  Still waiting... ({elapsed}s)")
        
        time.sleep(2)
    
    # Timeout reached
    if not db_healthy:
        print_warning("Database container did not become healthy in time")
    if not app_healthy:
        print_warning("Application container did not become healthy in time")
    
    return False


def show_container_logs(runtime: str, project_root: Path, lines: int = 50):
    """Show recent container logs for debugging."""
    compose_cmd = get_compose_command(runtime)
    
    print("\n" + "-" * 60)
    print("  Recent Container Logs")
    print("-" * 60 + "\n")
    
    try:
        subprocess.run(
            [*compose_cmd, "logs", "--tail", str(lines)],
            cwd=project_root,
            timeout=30
        )
    except (subprocess.TimeoutExpired, OSError) as e:
        print_error(f"Failed to retrieve logs: {e}")


def check_existing_containers(runtime: str, project_root: Path) -> tuple[bool, str]:
    """Check if containers already exist from a previous run."""
    compose_cmd = get_compose_command(runtime)
    
    try:
        result = subprocess.run(
            [*compose_cmd, "ps", "-a"],
            cwd=project_root,
            capture_output=True,
            text=True,
            timeout=10
        )
        
        if result.returncode == 0 and result.stdout.strip():
            # Check if there are any melodee containers
            if "melodee" in result.stdout.lower():
                return True, result.stdout
        
        return False, ""
    except (subprocess.TimeoutExpired, OSError):
        return False, ""


def print_next_steps(runtime: str, started: bool, healthy: bool = False):
    """Print next steps for the user."""
    compose_cmd = " ".join(get_compose_command(runtime))
    
    print("\n" + "-" * 60)
    print("  Next Steps")
    print("-" * 60 + "\n")
    
    if started and healthy:
        print_success("Melodee is up and running!")
        print_info("Access Melodee at: http://localhost:8080")
        print()
        print_info(f"Useful commands:")
        print(f"    {compose_cmd} logs -f        # View logs")
        print(f"    {compose_cmd} ps             # Check status")
        print(f"    {compose_cmd} down           # Stop containers")
        print(f"    {compose_cmd} build          # Rebuild image")
        print(f"    {compose_cmd} up -d          # Start containers")
    elif started:
        print_warning("Containers started but may not be fully healthy yet")
        print_info("Access Melodee at: http://localhost:8080")
        print_info("(May take a minute for the application to fully start)")
        print()
        print_info(f"Check status with:")
        print(f"    {compose_cmd} ps")
        print(f"    {compose_cmd} logs -f")
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
    check_only = "--check-only" in sys.argv
    force_overwrite = "--force" in sys.argv
    
    # Get project root first for preflight checks
    project_root = get_project_root()
    print(f"Project root: {project_root}")
    
    # Run preflight checks
    if not run_preflight_checks(project_root):
        if check_only:
            print_error("Preflight checks failed")
            sys.exit(1)
        print_warning("Some preflight checks failed, but continuing...")
        print_info("Fix the issues above for best results")
    else:
        print_success("All preflight checks passed!")
    
    if check_only:
        print_info("\n--check-only specified, exiting after checks")
        sys.exit(0)
    
    # Detect container runtime
    print("\nDetecting container runtime...")
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
            print_info("Or ensure podman-compose plugin is installed")
        else:
            print_info("Install Docker Compose: https://docs.docker.com/compose/install/")
        sys.exit(1)
    
    # Check for existing containers
    exists, status = check_existing_containers(runtime, project_root)
    if exists:
        print_warning("\nExisting Melodee containers found:")
        print(status)
        response = input("  Stop and remove existing containers? (y/N): ").strip().lower()
        if response == 'y':
            compose_cmd = get_compose_command(runtime)
            subprocess.run([*compose_cmd, "down", "-v"], cwd=project_root, timeout=60)
            print_success("Existing containers removed")
    
    # Create .env file
    print("\nSetting up environment...")
    if not create_env_file(project_root, overwrite=force_overwrite):
        sys.exit(1)
    
    # Ensure .env is gitignored
    ensure_gitignore_has_env(project_root)
    
    # Optionally start containers
    started = False
    healthy = False
    if start_after_setup:
        print("\nStarting containers...")
        started = start_containers(runtime, project_root)
        if started:
            print_success("Containers started!")
            
            # Wait for healthy status
            healthy = wait_for_healthy(runtime, project_root)
            
            if not healthy:
                print_warning("Containers may not be fully healthy")
                print_info("Showing recent logs for debugging:")
                show_container_logs(runtime, project_root, lines=30)
        else:
            print_error("Failed to start containers")
            print_info("Showing recent logs for debugging:")
            show_container_logs(runtime, project_root, lines=50)
    
    # Print next steps
    print_next_steps(runtime, started, healthy)
    
    return 0 if (not start_after_setup or started) else 1


if __name__ == "__main__":
    sys.exit(main())
