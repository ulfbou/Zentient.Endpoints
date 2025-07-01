# Dockerfile to build and pack Zentient.Endpoints supporting .NET 6, 8, 9

# Use the stable .NET 9.0 SDK image.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set environment variables to prevent telemetry and logo output
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true \
    DOTNET_NOLOGO=true

# Install GitVersion.Tool globally
RUN dotnet tool install --global GitVersion.Tool

# Install jq for JSON processing (used in some advanced scripts, might not be strictly needed by this Dockerfile directly)
RUN apt-get update && \
    apt-get install -y jq && \
    rm -rf /var/lib/apt/lists/*

# Set the working directory inside the container to /app.
WORKDIR /app

# Copy the entire repository content into the /app directory in the container.
COPY . .

# --- START DEBUGGING STEPS INSIDE DOCKER (Useful for initial setup) ---
# List contents of the /app directory (where your repo is copied)
RUN echo "--- Contents of /app (after COPY) ---" && ls -aF /app
RUN echo "--- Contents of /app/src (after COPY) ---" && ls -aF /app/src
RUN echo "--- Contents of /app/tests (after COPY) ---" && ls -aF /app/tests
# Display the content of the solution file as seen by Docker
RUN echo "--- Contents of Zentient.Endpoints.sln (inside Docker) ---" && cat Zentient.Endpoints.sln
# --- END DEBUGGING STEPS INSIDE DOCKER ---

# Restore dependencies for the solution.
RUN dotnet restore Zentient.Endpoints.sln

# Build the solution in Release configuration.
RUN dotnet build Zentient.Endpoints.sln -c Release

# Run tests for the solution.
# This will run tests for all projects referenced by the solution, including test projects.
RUN dotnet test Zentient.Endpoints.sln --no-build --configuration Release

# Pack all non-test, packable projects.
# This step needs to be dynamic to pack *all* your src/ libraries.
# We can't use the simple `dotnet pack src/Results/Zentient.Results.csproj` anymore.
# Instead, we will use a shell loop to iterate over identified projects.
# This assumes the Dockerfile is primarily for building/packaging ALL projects,
# not just those *modified* (that logic is in the CI workflow).

# Get all packable projects from src/ (excluding Tests.Shared)
RUN PROJECT_FILES=$(find src/ -name "*.csproj" ! -path "src/Zentient.Endpoints.Tests.Common/*" -print) && \
    echo "Identified projects for packing: $PROJECT_FILES" && \
    for PROJECT_PATH in $PROJECT_FILES; do \
        echo "Packing $PROJECT_PATH..."; \
        dotnet pack "$PROJECT_PATH" -c Release -o /artifacts --no-build; \
    done

# Final stage (optional, for local inspection): copy artifacts out
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS final

WORKDIR /app
COPY --from=build /artifacts ./

# Default command to list the built artifacts for verification
CMD ["ls", "-l", "/app"]
