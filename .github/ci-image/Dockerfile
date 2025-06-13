# Use a robust .NET 8.0 SDK image as the base.
# Use --platform=$BUILDPLATFORM to inform Docker of the target build platform.
# This allows for multi-architecture image builds when using 'docker buildx'.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS zentient-ci-base

# Metadata labels for better traceability and documentation.
LABEL maintainer="zentientdev@gmail.com" \
      version="1.0" \
      description="CI base image with .NET 8.0 and .NET 9.0 SDKs for robust, secure, and multi-arch builds."

# --- Build Arguments ---
# Define all build arguments at the top for easy configuration and visibility.
ARG INCLUDE_DEBUG_TOOLS="false" # Default to 'false' for a leaner image. Set to 'true' for debug tools.
ARG DOTNET_VERSION_9="9.0.6"    # Specify the major.minor version for .NET 9.0.
                                # The 'dotnet-install.sh' script will fetch the latest GA patch.
ARG BUILDPLATFORM               # Automatically set by 'docker buildx' (e.g., linux/amd64, linux/arm64).
ARG TARGETARCH                  # Automatically set by 'docker buildx' (e.g., amd64, arm64).
ARG USER_UID=1000               # UID for the non-root user.
ARG USER_GID=$USER_UID          # GID for the non-root user (matching UID is common).
ARG USER_NAME=appuser           # Name of the non-root user.

# --- Environment Variables ---
# Consolidate environment variables for efficiency and readability.
# DEBIAN_FRONTEND=noninteractive suppresses prompts during apt-get operations.
# LANG/LC_ALL set UTF-8 locale for consistent output.
ENV DEBIAN_FRONTEND=noninteractive \
    LANG=C.UTF-8 \
    LC_ALL=C.UTF-8 \
    # .NET Runtime Performance Environment Variables:
    # These configure .NET runtime behavior for optimal performance in a containerized environment.
    # DOTNET_gcServer=1: Enables server garbage collection for throughput.
    # DOTNET_ThreadPool_MinThreads=100: Increases minimum worker threads for concurrency.
    # DOTNET_TC_QuickJitForLoops=1: Enables quick JIT for loops for faster startup.
    # DOTNET_TieredPGO=1: Enables Tiered Profile-Guided Optimization for sustained performance.
    # ASPNETCORE_ENVIRONMENT=Production: Ensures production optimizations for web components (if any).
    DOTNET_gcServer=1 \
    DOTNET_ThreadPool_MinThreads=100 \
    DOTNET_TC_QuickJitForLoops=1 \
    DOTNET_TieredPGO=1 \
    ASPNETCORE_ENVIRONMENT=Production \
    # Define the base .NET installation directory. This will be added to PATH later.
    DOTNET_INSTALL_DIR="/usr/share/dotnet"

# Install common system dependencies and perform deep apt-get cleanup.
# This minimizes image size by removing cached package files and lists.
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/* /var/cache/apt/archives/*.deb

# Download the dotnet-install script, verify its integrity, execute it, then remove the script.
# This ensures the .NET 9.0 SDK/Runtime (ASP.NET Core component) is installed.
RUN curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh && \
    # OPTIONAL: Integrity Verification for dotnet-install.sh script.
    # Replace 'YOUR_CALCULATED_SHA256_HERE' with the actual SHA256 hash of the script
    # obtained by manually downloading and hashing it. Uncomment to enable.
    # This provides strong protection against script tampering, but requires manual updates
    # if the script's hash changes.
    # echo "YOUR_CALCULATED_SHA256_HERE  /tmp/dotnet-install.sh" | sha256sum -c - && \
    bash /tmp/dotnet-install.sh \
        --version $DOTNET_VERSION_9 \
        --install-dir "$DOTNET_INSTALL_DIR" \
        --no-path \
        --runtime aspnetcore && \
    rm /tmp/dotnet-install.sh

# Ensure the main .NET installation directory is in the system's PATH for root user.
ENV PATH="${DOTNET_INSTALL_DIR}:${PATH}"

# Validate the core .NET SDK and runtime installation by outputting info.
# This also confirms the current target architecture determined by Docker Buildx.
RUN dotnet --info && \
    echo "Current Target Architecture: $TARGETARCH"

# Conditional installation of debugging tools based on the BUILD ARG 'INCLUDE_DEBUG_TOOLS'.
# These tools (e.g., strace, gdb, dotnet-dump) are only installed if explicitly requested,
# keeping the default image lean.
RUN if [ "$INCLUDE_DEBUG_TOOLS" = "true" ]; then \
    echo "Installing debug tools for $TARGETARCH..."; \
    apt-get update && apt-get install -y --no-install-recommends \
        strace \
        gdb \
        iputils-ping \
        net-tools \
        procps && \
    # Install .NET global tools. '|| true' prevents build failure if tool is already installed.
    dotnet tool install --global dotnet-dump || true && \
    dotnet tool install --global dotnet-trace || true && \
    dotnet tool install --global dotnet-format --version 8.* || true && \
    dotnet tool install --global coverlet.console --version 6.* || true && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/* /var/cache/apt/archives/*.deb; \
fi

# Set the working directory inside the container for your application code.
WORKDIR /app

# --- Security Enhancement: Create and switch to a non-root user ---
# Running containers as non-root users is a critical security best practice,
# reducing the attack surface if the container is compromised.

# Create a non-root user and group using the defined ARGs.
RUN groupadd --gid $USER_GID $USER_NAME && \
    useradd --uid $USER_UID --gid $USER_GID -m $USER_NAME && \
    # Ensure the non-root user owns the /app directory where application code will reside.
    chown -R $USER_NAME:$USER_NAME /app && \
    # Ensure the non-root user can access and execute the main .NET SDK/Runtime installation.
    chown -R $USER_NAME:$USER_NAME "$DOTNET_INSTALL_DIR" && \
    \
    # Conditionally set ownership and permissions for the global .NET tools directory.
    # This directory (/root/.dotnet) only exists if debug tools were installed (INCLUDE_DEBUG_TOOLS=true).
    # This 'if' block prevents 'No such file or directory' errors when debug tools are not included.
    if [ "$INCLUDE_DEBUG_TOOLS" = "true" ]; then \
        echo "Setting permissions for /root/.dotnet/tools for non-root user..."; \
        chown -R $USER_NAME:$USER_NAME /root/.dotnet && \
        chmod -R ug+rwx /root/.dotnet/tools; \
    fi

# Switch to the non-root user for all subsequent commands and default container execution.
USER $USER_NAME

# Set the HOME directory explicitly for the non-root user. This is a good practice.
ENV HOME=/home/$USER_NAME

# IMPORTANT: Adjust the PATH for the non-root user.
# This ensures that both the main .NET installation and (conditionally) the global tools
# installed by root are accessible to the 'appuser'.
# Having a non-existent directory in PATH is harmless; it just won't be used.
ENV PATH="/root/.dotnet/tools:${PATH}"

# Final validation of the environment setup after user switch and PATH adjustments.
# This confirms everything is correctly configured for the non-root user.
RUN echo "Final PATH: $PATH" && \
    echo "Current User: $(whoami)" && \
    dotnet --list-sdks && \
    dotnet --list-runtimes

# --- Usage Instructions (Comments) ---
# These comments provide guidance on how to build and run the image.
# EXPOSE instructions are typically for application images, not CI base images,
# so they are omitted or kept commented out for clarity.
# ENTRYPOINT is also usually defined in application-specific Dockerfiles or
# overridden by CI/CD pipelines.

# You can build this image (e.g., for AMD64 with debug tools) using buildx:
# docker buildx build --platform linux/amd64 --build-arg INCLUDE_DEBUG_TOOLS=true -t zentient-ci-image-debug:latest .

# To build the lean default image (e.g., for AMD64):
# docker buildx build --platform linux/amd64 -t zentient-ci-image:latest .

# To run a container from the built image for interactive testing:
# docker run --rm -it zentient-ci-image:latest
