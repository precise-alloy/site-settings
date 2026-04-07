using NSubstitute;

namespace TuyenPham.SiteSettings.Tests.Services;

public class SettingsServiceClearCacheTests : SettingsServiceTestBase
{
    [Fact]
    public void ClearCache_RemovesMasterCacheKey()
    {
        var service = CreateService();

        service.ClearCache();

        CacheManager.Received().Remove("TuyenPham-SiteSettings");
    }

    [Fact]
    public void ClearCache_RemovesLanguageSettingsCacheKey()
    {
        var service = CreateService();

        service.ClearCache();

        CacheManager.Received().Remove("TuyenPham-SiteSettings-LanguageSettings");
    }

    [Fact]
    public void ClearCache_CallsRemoveTwice()
    {
        var service = CreateService();

        service.ClearCache();

        // Should remove both master cache key and language settings cache key
        CacheManager.Received(2).Remove(Arg.Any<string>());
    }
}
