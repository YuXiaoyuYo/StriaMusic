using StriaMusic.Desktop.Share.Storage;

namespace StriaMusic.Desktop.Share.Tests.Storage;

public class PreferencesTests : IDisposable
{
    private readonly HashSet<(string key, string sharedName)> _usedKeys = [];

    private void TrackKey(string key, string sharedName = "Default")
    {
        _usedKeys.Add((key, sharedName));
    }

    public void Dispose()
    {
        foreach (var (key, sharedName) in _usedKeys)
        {
            Preferences.Remove(key, sharedName);
            Preferences.Clear(sharedName);
        }
        _usedKeys.Clear();
    }

    /// <summary>
    /// 测试获取不存在的键时应返回默认值
    /// </summary>
    [Fact]
    public void Get_ReturnsDefaultValue_WhenKeyDoesNotExist()
    {
        var result = Preferences.Get("NonExistentKey", 42);
        Assert.Equal(42, result);
    }

    /// <summary>
    /// 测试获取已存在的键时应返回存储的值
    /// </summary>
    [Fact]
    public void Get_ReturnsStoredValue_WhenKeyExists()
    {
        const string key = "ExistingKey";
        TrackKey(key);
        
        Preferences.Set(key, 123);
        var result = Preferences.Get(key, 0);
        Assert.Equal(123, result);
    }

    /// <summary>
    /// 测试设置已存在的键时应覆盖原有值
    /// </summary>
    [Fact]
    public void Set_OverwritesExistingValue()
    {
        const string key = "KeyToOverwrite";
        TrackKey(key);

        Preferences.Set(key, 100);
        Preferences.Set(key, 200);
        var result = Preferences.Get(key, 0);
        Assert.Equal(200, result);
    }

    /// <summary>
    /// 测试获取不存在的分组中的键时应返回默认值
    /// </summary>
    [Fact]
    public void Get_ReturnsDefaultValue_WhenSharedNameDoesNotExist()
    {
        var result = Preferences.Get("KeyInNonExistentGroup", "default", "NonExistentGroup");
        Assert.Equal("default", result);
    }

    /// <summary>
    /// 测试在指定分组中存储值
    /// </summary>
    [Fact]
    public void Set_StoresValueInSpecifiedSharedName()
    {
        const string key = "SharedKey";
        const string sharedName = "CustomGroup";
        TrackKey(key, sharedName);

        Preferences.Set(key, true, sharedName);
        var result = Preferences.Get(key, false, sharedName);
        Assert.True(result);
    }

    /// <summary>
    /// 测试获取无法解析的值时应抛出异常
    /// </summary>
    [Fact]
    public void Get_ThrowsFormatException_WhenValueCannotBeParsed()
    {
        const string key = "InvalidIntKey";
        TrackKey(key);

        Preferences.Set(key, "NotAnInt");
        Assert.Throws<FormatException>(() => Preferences.Get(key, 0));
    }

    /// <summary>
    /// 测试设置空值时应存储空字符串
    /// </summary>
    [Fact]
    public void Set_HandlesNullValue()
    {
        const string key = "NullValueKey";
        TrackKey(key);

        Preferences.Set<string>(key, null!);
        var result = Preferences.Get(key, "default");
        Assert.Equal(string.Empty, result);
    }

    /// <summary>
    /// 测试移除不存在的键时应返回 false
    /// </summary>
    [Fact]
    public void Remove_ReturnsFalse_WhenKeyDoesNotExist()
    {
        var result = Preferences.Remove("NonExistentKey");
        Assert.False(result);
    }

    /// <summary>
    /// 测试移除存在的键时应返回 true 并成功移除
    /// </summary>
    [Fact]
    public void Remove_ReturnsTrue_AndRemovesKey_WhenKeyExists()
    {
        const string key = "KeyToRemove";
        TrackKey(key);

        Preferences.Set(key, "value");
        var result = Preferences.Remove(key);
        Assert.True(result);
        Assert.Equal("default", Preferences.Get(key, "default"));
    }

    /// <summary>
    /// 测试移除不存在的分组中的键时应返回 false
    /// </summary>
    [Fact]
    public void Remove_ReturnsFalse_WhenSharedNameDoesNotExist()
    {
        var result = Preferences.Remove("SomeKey", "NonExistentGroup");
        Assert.False(result);
    }
}
