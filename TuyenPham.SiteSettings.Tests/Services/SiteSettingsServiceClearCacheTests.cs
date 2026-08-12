using NSubstitute;
using EPiServer.Core;

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

    [Fact]
    public void ContentLanguageSettingsChanged_ForRootSettings_ClearsSettingsCaches()
    {
        var service = CreateService();

        service.ContentLanguageSettingsChanged(ContentReference.RootPage);

        CacheManager.Received().Remove("TuyenPham-SiteSettings");
        CacheManager.Received().Remove("TuyenPham-SiteSettings-LanguageSettings");
    }

    [Fact]
    public void ContentLanguageSettingsChanged_ForUnrelatedContent_DoesNotClearSettingsCaches()
    {
        var service = CreateService();

        service.ContentLanguageSettingsChanged(CreateContentReference(42));

        CacheManager.DidNotReceive().Remove(Arg.Any<string>());
    }
}
