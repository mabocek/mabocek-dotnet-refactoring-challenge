# Integration Test Results

## Command

```bash
docker-compose up --build tests
```

## Execution Results

```bash
cd /Users/matousbocek/repos/dotnet-refactoring-challenge && docker-compose up --build tests
[+] Building 45.3s (15/15) FINISHED                                                                                                                        docker:rancher-desktop
 => [tests internal] load build definition from Tests.Dockerfile                                                                                                             0.0s
 => => transferring dockerfile: 1.53kB                                                                                                                                       0.0s
 => [tests internal] load .dockerignore                                                                                                                                      0.0s
 => => transferring context: 380B                                                                                                                                            0.0s
 => [tests internal] load metadata for mcr.microsoft.com/dotnet/sdk:9.0-preview                                                                                              0.3s
 => [tests internal] load build context                                                                                                                                      0.0s
 => => transferring context: 1.00MB                                                                                                                                          0.0s
 => [tests  1/10] FROM mcr.microsoft.com/dotnet/sdk:9.0-preview@sha256:241df9f9fd6365ed075ffcf8db22e16276683feed4cc1caa6c3a4a631a38e5ae                                      0.0s
 => CACHED [tests  2/10] WORKDIR /app                                                                                                                                        0.0s
 => [tests  3/10] COPY . ./                                                                                                                                                  0.0s
 => [tests  4/10] RUN apt-get update &&     apt-get install -y curl gnupg apt-transport-https iputils-ping dnsutils netcat-openbsd unixodbc-dev bzip2 &&     mkdir -p /opt  11.7s
 => [tests  5/10] RUN chmod +x /app/.tools/init-db.sh                                                                                                                        0.3s 
 => [tests  6/10] RUN chmod +x /app/run-docker-tests.sh                                                                                                                      0.3s 
 => [tests  7/10] RUN chmod +x /app/.tools/check-sql-status.sh                                                                                                               0.4s 
 => [tests  8/10] RUN chmod +x /app/.tools/init-db-with-go-sqlcmd.sh                                                                                                         0.3s 
 => [tests  9/10] RUN dotnet restore                                                                                                                                        31.4s 
 => [tests 10/10] COPY ./RefactoringChallenge.Output/DatabaseSchema.sql /app/RefactoringChallenge.Output/DatabaseSchema.sql                                                  0.0s 
 => [tests] exporting to image                                                                                                                                               0.5s 
 => => exporting layers                                                                                                                                                      0.5s 
 => => writing image sha256:7fcecfa2b2862cfb87deb70ee5f8415ea6a88a15a9a644265927f2d9d8f31991                                                                                 0.0s 
 => => naming to docker.io/library/dotnet-refactoring-challenge-tests                                                                                                        0.0s
```

## Container Setup

```bash
[+] Running 2/0
 ✔ Container mssql                                 Running                                                                                                                   0.0s 
 ✔ Container dotnet-refactoring-challenge-tests-1  Recreated                                                                                                                 0.0s 
```

## Database Initialization

```
Attaching to tests-1
tests-1  | Current PATH: /opt/sqlcmd/bin:/opt/sqlcmd/bin:/opt/mssql-tools18/bin:/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin
tests-1  | /opt/sqlcmd/bin/sqlcmd
tests-1  | Using SQL command tool: /opt/sqlcmd/bin/sqlcmd
tests-1  | Waiting for SQL Server to start...
tests-1  | Checking SQL Server connection...
tests-1  |            
tests-1  | -----------
tests-1  |           1
tests-1  | 
tests-1  | (1 row affected)
tests-1  | Connection successful!
tests-1  | SQL Server is ready!
tests-1  | Waiting for database 'refactoringchallenge' to be created...
tests-1  | Checking if database 'refactoringchallenge' exists...
tests-1  | Database exists!
tests-1  | SQL Server and database are ready!
tests-1  | Initializing database with go-sqlcmd...
tests-1  | Starting database initialization with go-sqlcmd...
tests-1  | Current PATH: /opt/sqlcmd/bin:/opt/sqlcmd/bin:/opt/sqlcmd/bin:/opt/mssql-tools18/bin:/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin
tests-1  | /opt/sqlcmd/bin/sqlcmd
tests-1  | Using SQL command tool: /opt/sqlcmd/bin/sqlcmd
tests-1  | Waiting for SQL Server to start...
tests-1  | Attempt 1/30: Checking if SQL Server is ready...
tests-1  | SQL Server is ready!
tests-1  | Dropping database if it exists...
tests-1  | Creating database...
tests-1  | Creating schema...
tests-1  | Database initialization complete!
```

## Project Build and Test Execution

```
tests-1  | Running tests...
tests-1  |   Determining projects to restore...
tests-1  |   All projects are up-to-date for restore.
```

### Build Warnings

<details>
<summary>View Build Warnings (Click to expand)</summary>

```
tests-1  | /usr/share/dotnet/sdk/9.0.100-preview.7.24407.12/Sdks/Microsoft.NET.Sdk/targets/Microsoft.NET.RuntimeIdentifierInference.targets(326,5): message NETSDK1057: You are using a preview version of .NET. See: https://aka.ms/dotnet-support-policy [/app/RefactoringChallenge.Common/RefactoringChallenge.Common.csproj]
tests-1  | /usr/share/dotnet/sdk/9.0.100-preview.7.24407.12/Sdks/Microsoft.NET.Sdk/targets/Microsoft.NET.RuntimeIdentifierInference.targets(326,5): message NETSDK1057: You are using a preview version of .NET. See: https://aka.ms/dotnet-support-policy [/app/RefactoringChallenge.Tests/RefactoringChallenge.Tests.csproj]
tests-1  | /app/RefactoringChallenge.Common/Models/Product.cs(6,19): warning CS8618: Non-nullable property 'Name' must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring the property as nullable. [/app/RefactoringChallenge.Common/RefactoringChallenge.Common.csproj]
tests-1  | /app/RefactoringChallenge.Common/Models/Product.cs(7,19): warning CS8618: Non-nullable property 'Category' must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring the property as nullable. [/app/RefactoringChallenge.Common/RefactoringChallenge.Common.csproj]
tests-1  | /app/RefactoringChallenge.Common/Models/Customer.cs(6,19): warning CS8618: Non-nullable property 'Name' must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring the property as nullable. [/app/RefactoringChallenge.Common/RefactoringChallenge.Common.csproj]
tests-1  | /app/RefactoringChallenge.Common/Models/Customer.cs(7,19): warning CS8618: Non-nullable property 'Email' must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring the property as nullable. [/app/RefactoringChallenge.Common/RefactoringChallenge.Common.csproj]
tests-1  |   RefactoringChallenge.Common -> /app/RefactoringChallenge.Common/bin/Debug/net9.0/RefactoringChallenge.Common.dll
tests-1  | /usr/share/dotnet/sdk/9.0.100-preview.7.24407.12/Microsoft.TestPlatform.targets(46,5): warning : No test is available in /app/RefactoringChallenge.Common/bin/Debug/net9.0/RefactoringChallenge.Common.dll. Make sure that test discoverer & executors are registered and platform & framework version settings are appropriate and try again. [/app/RefactoringChallenge.Common/RefactoringChallenge.Common.csproj]
tests-1  |   Skipped! - Failed: 0, Passed: 0, Skipped: 0, Total: 0, Duration: [422ms]
tests-1  |   RefactoringChallenge.Orchestration -> /app/RefactoringChallenge.Orchestration/bin/Debug/net9.0/RefactoringChallenge.Orchestration.dll
tests-1  | /usr/share/dotnet/sdk/9.0.100-preview.7.24407.12/Microsoft.TestPlatform.targets(46,5): warning : No test is available in /app/RefactoringChallenge.Orchestration/bin/Debug/net9.0/RefactoringChallenge.Orchestration.dll. Make sure that test discoverer & executors are registered and platform & framework version settings are appropriate and try again. [/app/RefactoringChallenge.Orchestration/RefactoringChallenge.Orchestration.csproj]
tests-1  |   Skipped! - Failed: 0, Passed: 0, Skipped: 0, Total: 0, Duration: [685ms]
tests-1  | /root/.nuget/packages/microsoft.net.test.sdk/17.13.0/build/netcoreapp3.1/Microsoft.NET.Test.Sdk.Program.cs(4,41): warning CS7022: The entry point of the program is global code; ignoring 'AutoGeneratedProgram.Main(string[])' entry point. [/app/RefactoringChallenge.Output/RefactoringChallenge.Output.csproj]
tests-1  | /app/RefactoringChallenge.Output/Deprecated_CustomerOrderProcessorTests.cs(39,21): warning CS8602: Dereference of a possibly null reference. [/app/RefactoringChallenge.Output/RefactoringChallenge.Output.csproj]
tests-1  | /app/RefactoringChallenge.Output/Deprecated_CustomerOrderProcessor.cs(34,33): warning CS8600: Converting null literal or possible null value to non-nullable type. [/app/RefactoringChallenge.Output/RefactoringChallenge.Output.csproj]
tests-1  |   RefactoringChallenge.Output -> /app/RefactoringChallenge.Output/bin/Debug/net9.0/RefactoringChallenge.Output.dll
tests-1  | /usr/share/dotnet/sdk/9.0.100-preview.7.24407.12/Microsoft.TestPlatform.targets(46,5): warning : No test is available in /app/RefactoringChallenge.Output/bin/Debug/net9.0/RefactoringChallenge.Output.dll. Make sure that test discoverer & executors are registered and platform & framework version settings are appropriate and try again. [/app/RefactoringChallenge.Output/RefactoringChallenge.Output.csproj]
tests-1  |   Skipped! - Failed: 0, Passed: 0, Skipped: 0, Total: 0, Duration: [847ms]
tests-1  | /app/RefactoringChallenge.Tests/TestHelpers/EnhancedMockDb.cs(11,47): warning CS8765: Nullability of type of parameter 'value' doesn't match overridden member (possibly because of nullability attributes). [/app/RefactoringChallenge.Tests/RefactoringChallenge.Tests.csproj]
tests-1  | /app/RefactoringChallenge.Tests/TestHelpers/MockDb.cs(49,47): warning CS8765: Nullability of type of parameter 'value' doesn't match overridden member (possibly because of nullability attributes). [/app/RefactoringChallenge.Tests/RefactoringChallenge.Tests.csproj]
tests-1  | /app/RefactoringChallenge.Tests/TestHelpers/MockDb.cs(179,49): warning CS8765: Nullability of type of parameter 'value' doesn't match overridden member (possibly because of nullability attributes). [/app/RefactoringChallenge.Tests/RefactoringChallenge.Tests.csproj]
tests-1  | /app/RefactoringChallenge.Tests/TestHelpers/MockDb.cs(180,48): warning CS8765: Nullability of type of parameter 'value' doesn't match overridden member (possibly because of nullability attributes). [/app/RefactoringChallenge.Tests/RefactoringChallenge.Tests.csproj]
tests-1  | /app/RefactoringChallenge.Tests/TestHelpers/InMemoryDbConnection.cs(22,48): warning CS8764: Nullability of return type doesn't match overridden member (possibly because of nullability attributes). [/app/RefactoringChallenge.Tests/RefactoringChallenge.Tests.csproj]

<!-- Additional warnings omitted for brevity -->
```
</details>

### Test Execution Results  

<details>
<summary>View Database Connection Information (Click to expand)</summary>

```
tests-1  |   RefactoringChallenge.Tests -> /app/RefactoringChallenge.Tests/bin/Debug/net9.0/RefactoringChallenge.Tests.dll
tests-1  |   NUnit Adapter 4.6.0.0: Test execution started
tests-1  |   Running all tests in /app/RefactoringChallenge.Tests/bin/Debug/net9.0/RefactoringChallenge.Tests.dll
tests-1  |      NUnit3TestExecutor discovered 128 of 128 NUnit test cases using Current Discovery mode, Non-Explicit run
tests-1  |   Using connection string: Server=mssql,1433;Database=refactoringchallenge;User ID=sa;Password=RCPassword1!;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=60;
tests-1  |   
tests-1  |   Updated inventory in database for ProductId 1
tests-1  |   
tests-1  |   Using connection string: Server=mssql,1433;Database=refactoringchallenge;User ID=sa;Password=RCPassword1!;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=60;
tests-1  |   Successfully connected to SQL Server database.
tests-1  |   Successfully connected to SQL Server database.
tests-1  |   Successfully connected to SQL Server database.
tests-1  |   Successfully connected to SQL Server database.
tests-1  |   Successfully connected to SQL Server database.
tests-1  |   Successfully connected to SQL Server database.
tests-1  |   Successfully connected to SQL Server database.
tests-1  |   Successfully connected to SQL Server database.
tests-1  |   Successfully connected to SQL Server database.
tests-1  |   Successfully connected to SQL Server database.
tests-1  |   Successfully connected to SQL Server database.

<!-- Additional database connections omitted for brevity -->
```
</details>

### Test Summary

```bash
tests-1  |   NUnit Adapter 4.6.0.0: Test execution complete
tests-1  |   Passed! - Failed: 0, Passed: 128, Skipped: 0, Total: 128, Duration: [1s 613ms]
```

### Test Artifacts

```bash
tests-1  | Attachments:
tests-1  |   /app/RefactoringChallenge.Output/TestResults/3e80ba34-5007-4a7d-bbd9-4cef1d966a48/coverage.cobertura.xml
tests-1  |   /app/RefactoringChallenge.Orchestration/TestResults/04627539-8a8b-4d91-8576-80b4aa6eff51/coverage.cobertura.xml
tests-1  |   /app/RefactoringChallenge.Tests/TestResults/50e3e49c-9c67-4bc5-810d-559921f42f1c/coverage.cobertura.xml
tests-1  |   /app/RefactoringChallenge.Common/TestResults/7e8b28dc-9e1b-487e-9a45-550bbc2fa5ec/coverage.cobertura.xml
tests-1 exited with code 0
```

## Summary

The integration tests for the dotnet-refactoring-challenge project were executed successfully in a Docker container. 

- **Container used**: `mcr.microsoft.com/dotnet/sdk:9.0-preview`
- **Database**: SQL Server running in a separate container
- **Test framework**: NUnit 4.6.0.0
- **Test results**: All 128 tests passed successfully
- **Test duration**: 1.613 seconds
- **Code coverage**: Generated as Cobertura XML reports

### Notes

- The project is using .NET 9.0 Preview
- Some nullability warnings were generated during the build process
- The build process took approximately 45.3 seconds
- Test coverage reports were generated for all project components