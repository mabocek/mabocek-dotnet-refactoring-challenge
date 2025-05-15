# Running Tests

## Local Development
When developing locally, integration tests are excluded by default to speed up your workflow. To run tests locally:

```bash
# Run all tests except integration tests
./run-tests.sh

# Or directly using dotnet test
dotnet test --filter "Category!=Integration"
```

## Docker/CI Environment
When running in Docker or CI environments, all tests including integration tests are executed:

```bash
# Start all services including tests
docker-compose up --build

# Or just run the tests container
docker-compose up --build tests
```

You can also manually run the tests with integration tests included:

```bash
dotnet test
```

## VSCode Integration
The included .vscode/settings.json file configures the VSCode Test Explorer to exclude integration tests when running tests from the IDE.

## Custom Test Categories
To add a new test category or exclude specific tests from running locally, use the Category attribute:

```csharp
[Test]
[Category("Integration")] // This test will be excluded locally
public void MyIntegrationTest()
{
    // Test code
}

[Test]
[Category("Unit")] // This test will always run
public void MyUnitTest()
{
    // Test code
}
```
