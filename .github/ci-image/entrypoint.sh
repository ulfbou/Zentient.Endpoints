#!/bin/bash
set -e # Exit immediately if a command exits with a non-zero status

# Get the GITHUB_WORKSPACE and GITHUB_HOME variables from the environment.
# These are the paths mounted by the GitHub Actions runner from the host.
# Provide fallback defaults in case variables are not set (e.g., during local testing).
GITHUB_WORKSPACE_PATH="${GITHUB_WORKSPACE:-/__w/${GITHUB_REPOSITORY:-Zentient.Endpoints}}"
GITHUB_HOME_PATH="${HOME:-/github/home}"

# Ensure the non-root user has ownership of the mounted directories.
# These chown commands MUST run as root (which the ENTRYPOINT does by default).
echo "Correcting permissions for mounted volumes: ${GITHUB_WORKSPACE_PATH} and ${GITHUB_HOME_PATH}..."
chown -R "${USER_UID}:${USER_GID}" "${GITHUB_WORKSPACE_PATH}"
chown -R "${USER_UID}:${USER_GID}" "${GITHUB_HOME_PATH}"
echo "Permissions corrected. Executing original command as ${USER_NAME}..."

# Execute the original command passed to the container (e.g., the commands for the GHA step)
# `su-exec` securely drops privileges to the specified user and then runs the command.
exec su-exec "${USER_NAME}" "$@"
