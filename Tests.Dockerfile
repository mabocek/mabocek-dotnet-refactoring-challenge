FROM --platform=linux/arm64/v8 mcr.microsoft.com/dotnet/sdk:9.0-preview AS build

WORKDIR /app

# Copy everything
COPY . ./

# Install networking utilities and go-sqlcmd for ARM64
RUN apt-get update && \
    apt-get install -y curl gnupg apt-transport-https iputils-ping dnsutils netcat-openbsd unixodbc-dev bzip2 && \
    # Install go-sqlcmd
    mkdir -p /opt/sqlcmd/bin && \
    cd /opt/sqlcmd/bin && \
    export GOSQLCMD_VERSION=v1.8.2 && \
    curl -L -o sqlcmd.tar.bz2 https://github.com/microsoft/go-sqlcmd/releases/download/${GOSQLCMD_VERSION}/sqlcmd-linux-arm64.tar.bz2 && \
    tar -xjf sqlcmd.tar.bz2 && \
    chmod +x sqlcmd && \
    rm sqlcmd.tar.bz2 && \
    # Add to PATH
    echo 'export PATH="$PATH:/opt/sqlcmd/bin"' >> ~/.bashrc && \
    echo 'export PATH="$PATH:/opt/sqlcmd/bin"' >> ~/.profile

# Make scripts executable
RUN chmod +x /app/.tools/init-db.sh
RUN chmod +x /app/run-docker-tests.sh
RUN chmod +x /app/.tools/check-sql-status.sh
RUN chmod +x /app/.tools/init-db-with-go-sqlcmd.sh

# Restore dependencies
RUN dotnet restore

# Set environment variable to identify Docker environment
ENV DOCKER_CONTAINER=true
ENV PATH="/opt/sqlcmd/bin:/opt/mssql-tools18/bin:$PATH"

# Copy the SQL schema file to a specific location for direct usage
COPY ./RefactoringChallenge.Output/DatabaseSchema.sql /app/RefactoringChallenge.Output/DatabaseSchema.sql

# Use the run-docker-tests.sh script to wait for SQL Server and run tests
CMD ["/bin/bash", "/app/run-docker-tests.sh"]
