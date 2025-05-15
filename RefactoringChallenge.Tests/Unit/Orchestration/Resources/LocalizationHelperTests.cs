using System.Globalization;
using NUnit.Framework;
using RefactoringChallenge.Orchestration.Resources;

namespace RefactoringChallenge.Tests.Unit.Orchestration.Resources;

[TestFixture]
public class LocalizationHelperTests
{
    private CultureInfo _originalCulture;
    private CultureInfo _originalUICulture;

    [SetUp]
    public void Setup()
    {
        // Store the original culture settings to restore them after tests
        _originalCulture = CultureInfo.CurrentCulture;
        _originalUICulture = CultureInfo.CurrentUICulture;
    }

    [TearDown]
    public void Teardown()
    {
        // Restore original culture settings after each test
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.CurrentUICulture = _originalUICulture;

        // Reset the static _isInitialized flag through reflection to allow tests to work independently
        var field = typeof(LocalizationHelper).GetField("_isInitialized",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        field?.SetValue(null, false);
    }

    [Test]
    public void Initialize_WithSpecificCulture_SetsCultureCorrectly()
    {
        // Arrange
        string cultureName = "cs-CZ"; // Czech culture

        // Act
        LocalizationHelper.Initialize(cultureName);

        // Assert
        Assert.That(CultureInfo.CurrentCulture.Name, Does.StartWith("cs"));
        Assert.That(CultureInfo.CurrentUICulture.Name, Does.StartWith("cs"));
    }

    [Test]
    public void Initialize_WhenCalledTwice_OnlySetsOnce()
    {
        // Arrange
        LocalizationHelper.Initialize("en-US");

        // Act - trying to change to a different culture with second Initialize call
        LocalizationHelper.Initialize("cs-CZ");

        // Assert - should still be the first culture (en-US)
        Assert.That(CultureInfo.CurrentCulture.Name, Does.StartWith("en"));
    }

    [Test]
    public void Initialize_WithNullCulture_KeepsCurrentCulture()
    {
        // Arrange
        var expectedCulture = CultureInfo.CurrentCulture;

        // Act
        LocalizationHelper.Initialize(null);

        // Assert
        Assert.That(CultureInfo.CurrentCulture, Is.EqualTo(expectedCulture));
    }

    [Test]
    public void SetCulture_ChangesCurrentCulture()
    {
        // Arrange
        string cultureName = "cs";

        // Act
        LocalizationHelper.SetCulture(cultureName);

        // Assert
        Assert.That(CultureInfo.CurrentCulture.Name, Does.StartWith("cs"));
        Assert.That(CultureInfo.CurrentUICulture.Name, Does.StartWith("cs"));
    }

    [Test]
    public void SetCulture_CanChangeMultipleTimes()
    {
        // Arrange & Act
        LocalizationHelper.SetCulture("cs");

        // Assert first change
        Assert.That(CultureInfo.CurrentCulture.Name, Does.StartWith("cs"));

        // Act again
        LocalizationHelper.SetCulture("en-US");

        // Assert second change
        Assert.That(CultureInfo.CurrentCulture.Name, Does.StartWith("en"));
    }

    [Test]
    public void SetCulture_WithInvalidCulture_ThrowsCultureNotFoundException()
    {
        // Since the CultureNotFoundException might be system dependent,
        // let's modify our approach to test the exception handling

        // Option 1: Change the test to verify exception is thrown with a very invalid culture
        // This should work on all systems
        // Arrange
        string invalidCulture = "ThisIsDefinitelyNotACultureName!@#$%^&";

        // Act & Assert
        var exception = Assert.Throws<CultureNotFoundException>(() => LocalizationHelper.SetCulture(invalidCulture));
        Assert.That(exception.Message, Does.Contain(invalidCulture));
    }
}
