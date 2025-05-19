namespace StriaMusic.Desktop.Share.Storage;

public static class Path
{
    public static string GetConfigFilePath(string? fileName = null, string? path = null) => GetFilePath(ConfigFolder, fileName, path);
    public static string GetLocalFilePath(string? fileName = null, string? path = null) => GetFilePath(LocalFolder, fileName, path);
    public static string GetMusicFilePath(string? fileName = null, string? path = null) => GetFilePath(MusicFolder, fileName, path);
    
    private static string? _configFolder;
    private static string? _localFolder;
    private static string? _musicFolder;

    private static string ConfigFolder => 
        _configFolder ??= GetFolderPath(ref _configFolder, Environment.SpecialFolder.ApplicationData);
    private static string LocalFolder =>
        _localFolder ??= GetFolderPath(ref _localFolder, Environment.SpecialFolder.LocalApplicationData);
    private static string MusicFolder => 
        _musicFolder ??= GetFolderPath(ref _musicFolder, Environment.SpecialFolder.MyMusic);
    private static string GetFolderPath(ref string? folder, Environment.SpecialFolder specialFolder)
    {
        if (!string.IsNullOrEmpty(folder))
            return folder;

        folder = System.IO.Path.Combine(Environment.GetFolderPath(specialFolder), nameof(StriaMusic));

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    private static string GetFilePath(string rootFolder, string? fileName, string? path = null) =>
        string.IsNullOrEmpty(fileName) ? rootFolder :
            string.IsNullOrEmpty(path) ?
                System.IO.Path.Combine(rootFolder, fileName) :
                System.IO.Path.Combine(rootFolder, path, fileName);
}
