# Dockerfile to build and pack ulfbou/Zentient.Endpoints supporting .NET 8, 9

# Use the stable .NET 9.0 SDK image.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set environment variables to prevent telemetry and logo output
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true \
    DOTNET_NOLOGO=true \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true

# Install .NET 8.0 SDK via APT package manager
# This is more reliable for stable/LTS versions on Debian-based images.
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        apt-transport-https \
        ca-certificates \
        curl \
        gnupg \
        git \
        jq && \
    # Add Microsoft's GPG key using the safer curl + gpg method
    # IMPORTANT: Ensure the key is placed in /usr/share/keyrings/ as specified in prod.list
    mkdir -p /usr/share/keyrings && \
    curl -sSL https://packages.microsoft.com/keys/microsoft.asc -o microsoft.asc && \
    gpg --dearmor -o /usr/share/keyrings/microsoft-prod.gpg microsoft.asc && \
    rm microsoft.asc && \
    # Add Microsoft's package list for Debian 12 (assuming the base is Debian 12)
    # This prod.list explicitly references the key at /usr/share/keyrings/microsoft-prod.gpg
    wget -O - https://packages.microsoft.com/config/debian/12/prod.list | tee /etc/apt/sources.list.d/microsoft-prod.list && \
    # Update apt-get and install .NET 8.0 SDK
    apt-get update && \
    apt-get install -y --no-install-recommends dotnet-sdk-8.0 && \
    # Clean up apt caches to keep the image size down
    rm -rf /var/lib/apt/lists/*

# Install GitVersion.Tool globally
# Ensure /root/.dotnet/tools is on PATH for dotnet tools
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet tool install --global GitVersion.Tool

WORKDIR /app

# Necessary for GitVersion when WORKDIR changes relative to the git root.
RUN git config --global --add safe.directory /app

# Copy the entire repository to ensure .git directory and Directory.Build.props are present
COPY . .

# Copy all MSBuild configuration files from repo root
COPY *.props ./
COPY *.targets ./

# IMPORTANT: Fix path separators in the solution file for Linux compatibility
# This command replaces all '\' with '/' in the .sln file
RUN sed -i 's|\\|/|g' Zentient.Endpoints.sln

# Restore dependencies
RUN dotnet restore "Zentient.Endpoints.sln"

# Pass the ZENTIENT_VERSION_FINAL_OVERRIDE build argument to this stage
ARG ZENTIENT_VERSION_FINAL_OVERRIDE

# Calculate version and export as an environment variable for subsequent steps
RUN if [ -z "$ZENTIENT_VERSION_FINAL_OVERRIDE" ]; then \
        export CALCULATED_VERSION=$(dotnet-gitversion /output json | jq -r '.SemVer'); \
        echo "Calculated version: $CALCULATED_VERSION"; \
        export ZENTIENT_VERSION_FINAL="$CALCULATED_VERSION"; \
    else \
        echo "Using provided version override: $ZENTIENT_VERSION_FINAL_OVERRIDE"; \
        echo "Using final version for build: $ZENTIENT_VERSION_FINAL_OVERRIDE"; \
        export ZENTIENT_VERSION_FINAL="$ZENTIENT_VERSION_FINAL_OVERRIDE"; \
    fi && \
    # Set the environment variable for this and subsequent RUN commands in this stage
    echo "export ZENTIENT_VERSION_FINAL=$ZENTIENT_VERSION_FINAL" >> /etc/profile.d/zentient_version.sh && \
    # Make sure it's available in the current shell context
    export ZENTIENT_VERSION_FINAL="$ZENTIENT_VERSION_FINAL"

# Build the solution in Release configuration
RUN dotnet build "Zentient.Endpoints.sln" -c Release --no-restore \
    /p:IsDockerBuild=true \
    /p:ContinuousIntegrationBuild=true

# ONLY RUN TESTS FOR ZENTIENT.ENDPOINTS.TESTS
# This will only execute tests within that specific project.
# Store test results in a well-known path within the container.
RUN dotnet test "tests/Zentient.Endpoints.Tests/Zentient.Endpoints.Tests.csproj" --no-build --configuration Release --logger "trx;LogFileName=test-results.trx" --collect "XPlat Code Coverage" --results-directory "/app/test-results"

# Create a directory for NuGet package artifacts
RUN mkdir -p /app/nuget-packages

# Pack ONLY Zentient.Endpoints.csproj for the initial beta release
# Store packed NuGet packages in a well-known path within the container.
RUN echo "Packing src/Zentient.Endpoints/Zentient.Endpoints.csproj..."; \
    dotnet pack "src/Zentient.Endpoints/Zentient.Endpoints.csproj" -c Release -o /app/nuget-packages --no-build \
    /p:ContinuousIntegrationBuild=true \
    /p:Version=$ZENTIENT_VERSION_FINAL # Ensure the final version from GitVersion is used

# Final stage: copy artifacts out (using scratch for smallest image for artifacts)
FROM scratch AS artifacts
WORKDIR /app

# Copy the NuGet packages and test results from the build stage
COPY --from=build /app/nuget-packages /app/nuget-packages
COPY --from=build /app/test-results /app/test-results

# Default command to list the built artifacts for verification (useful for debugging)
CMD ["ls", "-lR", "/app"]
