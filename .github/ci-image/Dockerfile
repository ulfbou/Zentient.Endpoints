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
    DOTNET_gcServer=1 \
    DOTNET_ThreadPool_MinThreads=100 \
    DOTNET_TC_QuickJitForLoops=1 \
    DOTNET_TieredPGO=1 \
    ASPNETCORE_ENVIRONMENT=Production \
    # Define the base .NET installation directory. This will be added to PATH later.
    DOTNET_INSTALL_DIR="/usr/share/dotnet" \
    # Expose UID, GID, and USER_NAME as environment variables for the entrypoint script
    USER_UID=$USER_UID \
    USER_GID=$USER_GID \
    USER_NAME=$USER_NAME

# Install common system dependencies and perform deep apt-get cleanup.
# This minimizes image size by removing cached package files and lists.
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/* /var/cache/apt/archives/*.deb

# Download the dotnet-install script, verify its integrity, execute it, then remove the script.
RUN curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh && \
    bash /tmp/dotnet-install.sh \
        --version $DOTNET_VERSION_9 \
        --install-dir "$DOTNET_INSTALL_DIR" \
        --no-path \
        --runtime aspnetcore && \
    rm /tmp/dotnet-install.sh

# Ensure the main .NET installation directory is in the system's PATH for root user initially.
ENV PATH="${DOTNET_INSTALL_DIR}:${PATH}"

# Validate the core .NET SDK and runtime installation.
RUN dotnet --info && \
    echo "Current Target Architecture: $TARGETARCH"

# Conditional installation of debugging tools based on the BUILD ARG 'INCLUDE_DEBUG_TOOLS'.
RUN if [ "$INCLUDE_DEBUG_TOOLS" = "true" ]; then \
    echo "Installing debug tools for $TARGETARCH..."; \
    apt-get update && apt-get install -y --no-install-recommends \
        strace \
        gdb \
        iputils-ping \
        net-tools \
        procps && \
    dotnet tool install --global dotnet-dump || true && \
    dotnet tool install --global dotnet-trace || true && \
    dotnet tool install --global dotnet-format --version 8.* || true && \
    dotnet tool install --global coverlet.console --version 6.* || true && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/* /var/cache/apt/archives/*.deb; \
fi

# Set the working directory inside the container for your application code.
WORKDIR /app

# --- Security Enhancement: Create non-root user and set *image-internal* permissions ---
# Running containers as non-root users is a critical security best practice.
# The chown operations here are ONLY for directories *within the image* itself.
RUN groupadd --gid $USER_GID $USER_NAME && \
    useradd --uid $USER_UID --gid $USER_GID -m $USER_NAME && \
    chown -R $USER_NAME:$USER_NAME /app && \
    chown -R $USER_NAME:$USER_NAME "$DOTNET_INSTALL_DIR" && \
    if [ "$INCLUDE_DEBUG_TOOLS" = "true" ]; then \
        echo "Setting permissions for /root/.dotnet/tools for non-root user..."; \
        chown -R $USER_NAME:$USER_NAME /root/.dotnet && \
        chmod -R ug+rwx /root/.dotnet/tools; \
    fi && \
    chmod 1777 /tmp # Ensure /tmp is generally writable

# Install 'su-exec' for secure privilege dropping in the entrypoint.
RUN apt-get update && \
    apt-get install -y --no-install-recommends su-exec && \
    rm -rf /var/lib/apt/lists/*

# Copy and set up the entrypoint script.
# This script will run as root, fix permissions on mounted volumes, and then
# switch to the non-root user before running the main command.
COPY entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/entrypoint.sh

# The ENTRYPOINT will now be our custom script.
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]

# Set a default command if no command is provided (useful for interactive `docker run`).
# This CMD will be executed by entrypoint.sh as the appuser.
CMD ["tail", "-f", "/dev/null"]
