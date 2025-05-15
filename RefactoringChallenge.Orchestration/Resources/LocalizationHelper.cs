using System.Globalization;

namespace RefactoringChallenge.Orchestration.Resources;

/// <summary>
/// Helper class for localization settings
/// </summary>
public static class LocalizationHelper
{
    private static bool _isInitialized = false;

    /// <summary>
    /// Initializes the localization settings for the application
    /// </summary>
    /// <param name="culture">The culture to use (e.g., "en", "cs", etc.). If null, the current culture is used.</param>
    public static void Initialize(string? culture = null)
    {
        if (_isInitialized)
        {
            return;
        }

        if (!string.IsNullOrEmpty(culture))
        {
            var cultureInfo = new CultureInfo(culture);
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
        }

        _isInitialized = true;
    }

    /// <summary>
    /// Sets the current culture
    /// </summary>
    /// <param name="culture">The culture code (e.g., "en", "cs", etc.)</param>
    public static void SetCulture(string culture)
    {
        var cultureInfo = new CultureInfo(culture);
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
    }
}
