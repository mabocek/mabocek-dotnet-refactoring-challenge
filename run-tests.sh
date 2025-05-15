#!/bin/bash

# Get the directory of the script
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Detect if running in Docker
if [ -f "/.dockerenv" ] || [ -n "$DOCKER_CONTAINER" ]; then
  echo "Running in Docker environment - including integration tests"
  # Run all tests in Docker
  dotnet test
else
  echo "Running in local environment - excluding integration tests"
  # Directly use a filter expression to exclude Integration tests
  # This is more reliable than using runsettings files
  dotnet test --filter "Category!=Integration"
fi
