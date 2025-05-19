using System.Text.Json;
using System.Text.Json.Serialization;

namespace StriaMusic.Desktop.Share.Storage;

/// <summary>
/// 提供应用程序首选项的存储和检索功能。
/// 使用 JSON 文件持久化存储键值对数据。
/// </summary>
public static class Preferences
{
    /// <summary>
    /// 获取指定键的值，如果键不存在则返回默认值。
    /// </summary>
    /// <typeparam name="T">值的类型，支持 int、bool 和 string</typeparam>
    /// <param name="key">要获取的设置项的键</param>
    /// <param name="defaultValue">如果键不存在时返回的默认值</param>
    /// <param name="sharedName">存储分组名称，默认为 "Default"</param>
    /// <returns>存储的值或默认值</returns>
    public static T Get<T>(string key, T defaultValue, string sharedName = "Default")
    {
        if (!PreferencesData.TryGetValue(sharedName, out var item) ||
            !item.Values.TryGetValue(key, out var strValue))
        {
            return defaultValue;
        }

        return ParseValue(strValue, defaultValue);
    }

    /// <summary>
    /// 设置指定键的值并保存到存储文件中。
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">要设置的设置项的键</param>
    /// <param name="value">要存储的值</param>
    /// <param name="sharedName">存储分组名称，默认为 "Default"</param>
    public static void Set<T>(string key, T value, string sharedName = "Default")
    {
        if (!PreferencesData.TryGetValue(sharedName, out var item))
        {
            item = new Item();
            PreferencesData[sharedName] = item;
        }

        item.Values[key] = ConvertValue(value);
        SaveJson();
    }

    /// <summary>
    /// 移除指定键。
    /// </summary>
    /// <param name="key">要移除的设置项的键</param>
    /// <param name="sharedName">存储分组名称，默认为 "Default"</param>
    /// <returns>如果成功移除返回 true，如果键不存在返回 false</returns>
    public static bool Remove(string key, string sharedName = "Default")
    {
        if (!PreferencesData.TryGetValue(sharedName, out var item))
            return false;

        var removed = item.Values.Remove(key);
        if (removed) SaveJson();
        return removed;
    }

    /// <summary>
    /// 清除指定分组或所有分组的首选项数据。
    /// </summary>
    /// <param name="sharedName">可选的存储分组名称。如果未指定，则清除所有分组</param>
    /// <returns>如果成功清除返回 true，如果指定的分组不存在返回 false</returns>
    public static bool Clear(string? sharedName = null)
    {
        if (sharedName is null)
        {
            _preferencesData = new Dictionary<string, Item>();
            SaveJson();
            return true;
        }

        if (!PreferencesData.Remove(sharedName))
            return false;

        SaveJson();
        return true;
    }

    private static void SaveJson()
    {
        File.WriteAllText(PreferencesFilePath,
            JsonSerializer.Serialize(PreferencesData, PreferencesJsonContext.Default.DictionaryStringItem));
    }

    private static T ParseValue<T>(string str, T defaultValue)
    {
        if (typeof(T) == typeof(int))
            return (T)(object)int.Parse(str);
        if (typeof(T) == typeof(bool))
            return (T)(object)bool.Parse(str);
        if (typeof(T) == typeof(string))
            return (T)(object)str;
        
        return defaultValue;
    }

    private static string ConvertValue<T>(T value)
    {
        return value?.ToString() ?? string.Empty;
    }

    private static string PreferencesFilePath => Path.GetLocalFilePath("Preferences.dat");

    private static Dictionary<string, Item> PreferencesData =>
        _preferencesData ??= LoadOrCreate();

    private static Dictionary<string, Item>? _preferencesData;

    private static Dictionary<string, Item> LoadOrCreate()
    {
        if (!File.Exists(PreferencesFilePath))
            return new Dictionary<string, Item>();

        try
        {
            var json = File.ReadAllText(PreferencesFilePath);
            return JsonSerializer.Deserialize(json, PreferencesJsonContext.Default.DictionaryStringItem)
                   ?? new Dictionary<string, Item>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, Item>();
        }
    }

    /// <summary>
    /// 表示存储在首选项中的一组键值对
    /// </summary>
    public class Item
    {
        public Dictionary<string, string> Values { get; set; } = new();
    }
}

/// <summary>
/// System.Text.Json 源生成器上下文，用于 AOT 友好序列化。
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, Preferences.Item>))]
[JsonSerializable(typeof(Preferences.Item))]
partial class PreferencesJsonContext : JsonSerializerContext;
