# Dockerfile to build and pack ulfbou/Zentient.Endpoints supporting .NET 8, 9

# Use the stable .NET 9.0 SDK image.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set environment variables to prevent telemetry and logo output
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true \
    DOTNET_NOLOGO=true \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true

# Install GitVersion.Tool globally
RUN dotnet tool install --global GitVersion.Tool

# Add the .NET tools directory to the PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

# Install git and jq for version calculation and JSON processing
RUN apt-get update && \
    apt-get install -y git jq && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /src

# Necessary for GitVersion when WORKDIR changes relative to the git root.
RUN git config --global --add safe.directory /src

# Copy the entire repository to ensure .git directory and Directory.Build.props are present
COPY . .

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
        export ZENTIENT_VERSION_FINAL="$ZENTIENT_VERSION_FINAL_OVERRIDE"; \
    fi && \
    echo "Using final version for build: $ZENTIENT_VERSION_FINAL" && \
    # Set the environment variable for this and subsequent RUN commands in this stage
    echo "export ZENTIENT_VERSION_FINAL=$ZENTIENT_VERSION_FINAL" >> /etc/profile.d/zentient_version.sh && \
    # Make sure it's available in the current shell context
    export ZENTIENT_VERSION_FINAL="$ZENTIENT_VERSION_FINAL"

# --- REVISED FIX: Remove /p: parameters, rely on Directory.Build.props and ENV ---
# Build the solution in Release configuration
RUN dotnet build "Zentient.Endpoints.sln" -c Release --no-restore \
    /p:IsDockerBuild=true \
    /p:ContinuousIntegrationBuild=true

# Run tests
RUN dotnet test "Zentient.Endpoints.sln" --no-build --configuration Release

# Create a directory for artifacts
RUN mkdir -p /artifacts

# Pack all non-test, packable projects
RUN find src/ -maxdepth 2 -name "*.csproj" \
    ! -path "src/Zentient.Endpoints.Tests.Shared/*" \
    ! -path "src/Zentient.Analyzers/*" \
    ! -path "src/*/*Tests.csproj" \
    -print0 | while IFS= read -r -d $'\0' PROJECT_PATH; do \
        echo "Packing $PROJECT_PATH..."; \
        dotnet pack "$PROJECT_PATH" -c Release -o /artifacts --no-build \
        /p:ContinuousIntegrationBuild=true; \
    done

# Final stage: copy artifacts out (using scratch for smallest image for artifacts)
FROM scratch AS artifacts
WORKDIR /app
COPY --from=build /artifacts .

# Default command to list the built artifacts for verification
CMD ["ls", "-l", "/app"]
