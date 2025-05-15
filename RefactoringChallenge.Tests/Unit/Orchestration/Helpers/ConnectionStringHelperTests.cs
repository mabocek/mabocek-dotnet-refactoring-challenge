using NUnit.Framework;
using RefactoringChallenge.Orchestration.Helpers;

namespace RefactoringChallenge.Tests.Unit.Orchestration.Helpers;

[TestFixture]
public class ConnectionStringHelperTests
{
    [Test]
    public void MaskConnectionString_WithPassword_ReturnsPasswordMasked()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=testdb;User ID=testuser;Password=Secret123!";

        // Act
        var result = ConnectionStringHelper.MaskConnectionString(connectionString);

        // Assert
        Assert.That(result, Does.Contain("Server=localhost"));
        Assert.That(result, Does.Contain("Database=testdb"));
        Assert.That(result, Does.Contain("User ID=***masked***"));
        Assert.That(result, Does.Contain("Password=*****"));
        Assert.That(result, Does.Not.Contain("Secret123!"));
    }

    [Test]
    public void MaskConnectionString_WithPwd_ReturnsPasswordMasked()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=testdb;Uid=testuser;Pwd=Secret123!";

        // Act
        var result = ConnectionStringHelper.MaskConnectionString(connectionString);

        // Assert
        Assert.That(result, Does.Contain("Server=localhost"));
        Assert.That(result, Does.Contain("Database=testdb"));
        Assert.That(result, Does.Contain("Uid=***masked***"));
        Assert.That(result, Does.Contain("Pwd=*****"));
        Assert.That(result, Does.Not.Contain("Secret123!"));
    }

    [Test]
    public void MaskConnectionString_WithNullConnectionString_ReturnsAppropriateMessage()
    {
        // Arrange
        string? connectionString = null;

        // Act
        var result = ConnectionStringHelper.MaskConnectionString(connectionString!);

        // Assert
        Assert.That(result, Is.EqualTo("[connection string is null or empty]"));
    }

    [Test]
    public void MaskConnectionString_WithEmptyConnectionString_ReturnsAppropriateMessage()
    {
        // Arrange
        var connectionString = "";

        // Act
        var result = ConnectionStringHelper.MaskConnectionString(connectionString);

        // Assert
        Assert.That(result, Is.EqualTo("[connection string is null or empty]"));
    }

    [Test]
    public void MaskConnectionString_WithInvalidFormatParts_HandlesThem()
    {
        // Arrange
        var connectionString = "Server=localhost;InvalidPart;Database=testdb;User ID=testuser;Password=Secret123!";

        // Act
        var result = ConnectionStringHelper.MaskConnectionString(connectionString);

        // Assert
        Assert.That(result, Does.Contain("Server=localhost"));
        Assert.That(result, Does.Contain("InvalidPart"));
        Assert.That(result, Does.Contain("Database=testdb"));
        Assert.That(result, Does.Contain("User ID=***masked***"));
        Assert.That(result, Does.Contain("Password=*****"));
        Assert.That(result, Does.Not.Contain("Secret123!"));
    }
}
