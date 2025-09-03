#if BROWSERWASM
using Log = System.Console;
#else
using Log = System.Diagnostics.Debug;
#endif

namespace P42.Uno;

public static partial class WebView2Extensions
{
    
    #region Public
    public static async Task EnableProjectContentFolder(string projectFolder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectFolder);
        projectFolder = projectFolder.Trim(DirectorySeparators).Replace('\\', '/');
        
        if (SplitPathSegments(projectFolder) is { Length: > 1 })
            throw new ArgumentException("Only folders in project root are allowed", nameof(projectFolder));

        #if BROWSERWASM
        var assets = await WasmWebViewExtensions.GetAssetFilesAsync();
        if (assets is null)
            throw new Exception("Unable to get WASM asset files from package");

        var pFolder = projectFolder.Trim(DirectorySeparators) + '/';
        if (assets.Any(asset => asset.StartsWith(pFolder)))
            return;

        throw new DirectoryNotFoundException($"Project Content Folder Not Found: [{projectFolder}]");
        #else
        var fullFolderPath = Path.Combine(VirtualHost.ContentRoot, projectFolder);
        
        if (!ProjectFolderExists(fullFolderPath))
        {
            CheckForProjectFolderRecursively(fullFolderPath, out var result);
            throw new DirectoryNotFoundException($"Project Content Folder Not Found: [{fullFolderPath}] : [{result}]");
        }

        VirtualHost.LocalFolders.AddDistinct(projectFolder);
        await Task.CompletedTask;
        #endif
        
    }

    public static async Task NavigateToProjectContentFileAsync(this WebView2 webView2, string projectContentFilePath)
        => webView2.Source = await ProjectContentFileUriAsync(projectContentFilePath);

    public static async Task<Uri> ProjectContentFileUriAsync(string projectContentFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectContentFilePath);
        projectContentFilePath = projectContentFilePath.TrimStart(DirectorySeparators).Replace('\\', '/');

        var urlAndQuery = projectContentFilePath.Split('?');
        projectContentFilePath = urlAndQuery[0];
        var query = urlAndQuery.Length > 1 
            ? urlAndQuery[1]
            : string.Empty;

        // All paths without an extension are assumed to be directories -
        //  assume we're looking for index.html
        if (projectContentFilePath.EndsWith(Path.DirectorySeparatorChar) || projectContentFilePath.EndsWith(Path.AltDirectorySeparatorChar))
            projectContentFilePath += "index.html";

        if (string.IsNullOrWhiteSpace(Path.GetExtension(projectContentFilePath)))
            projectContentFilePath += Path.DirectorySeparatorChar + "index.html";

        var projectFolder = Path.GetDirectoryName(projectContentFilePath);
        if (string.IsNullOrWhiteSpace(projectFolder))
            throw new ArgumentException("Root project folder is not allowed.");

        #if BROWSERWASM
        var assets = await WasmWebViewExtensions.GetAssetFilesAsync();
        if (assets is null)
            throw new Exception("Unable to get WASM asset files from package");

        if (!assets.Any(asset => asset.StartsWith(projectContentFilePath)))
            throw new FileNotFoundException($"Project Content File Not Found: [{projectContentFilePath}] ");
        #else
        var fullFilePath = Path.Combine(VirtualHost.ContentRoot, projectContentFilePath);        
        if (!ProjectFileExists(fullFilePath))
        {
            CheckForProjectFileRecursively(fullFilePath, out var result);
            throw new FileNotFoundException($"Project Content File Not Found: [{fullFilePath}] : [{result}]");
        }

        var rootProjectFolder = SplitPathSegments(projectFolder)[0];
        VirtualHost.LocalFolders.AddDistinct(rootProjectFolder);
        #endif
        if (!string.IsNullOrWhiteSpace(query))
            projectContentFilePath += $"?{query}";

        #if BROWSERWASM
        var url =  $"{VirtualHost.HostUrl}{projectContentFilePath}";
        #else
        var url =  $"{VirtualHost.HostUrl}/{projectContentFilePath}";
        await Task.CompletedTask;
        #endif
        
        return new Uri(url);
    }
    
    #endregion Public


    #region Internal 
    internal static bool CheckForProjectFolderRecursively(string path, out string result)
    {
        result = $"FOUND: {path}";
        // Normalize path separators
        path = Path.GetFullPath(path);

        var current = Path.GetPathRoot(path)!; // e.g. "C:\"
        var segments = path.Split(DirectorySeparators, StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in segments)
        {
            current = Path.Combine(current, part);

            if (ProjectFolderExists(current))
                continue;

            result = $"MISSING: {current}";
            return false;
        }

        return true;
    }

    internal static bool CheckForProjectFileRecursively(string path, out string result)
    {
        result = $"FOUND: {path}";

        var folderPath = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(folderPath) && !CheckForProjectFolderRecursively(folderPath, out result))
            return false;

        if (ProjectFileExists(path))
            return true;

        var files = Directory.GetFiles(folderPath ?? "");
        foreach (var file in files) 
            Log.WriteLine($"\t\t{folderPath}/{file}");

        result = $"MISSING: {path}";
        return false;
    }

#if BROWSERWASM
    private static readonly HttpClient HttpClient = new HttpClient();
#endif

    internal static bool ProjectFolderExists(string fullFolderPath)
    {
        #if __ANDROID__
        try
        {
            var files = VirtualHost.Assets.List(fullFolderPath);
            if (files is null || files.Length == 0)
                return false;
            return true;
        }
        catch 
        { 
            return false; 
        }
        #elif BROWSERWASM
        throw new NotSupportedException($"{nameof(ProjectFolderExists)} is not supported on this platform: {Environment.OSVersion.Platform}.");
        #else
        return Directory.Exists(fullFolderPath);
        #endif
    }

    internal static bool ProjectFileExists(string fullFilePath)
    {
        #if __ANDROID__
        var folderPath = Path.GetDirectoryName(fullFilePath);
        try
        {
            using var stream = VirtualHost.Assets.Open(fullFilePath);
            return stream != null;
            /*
            var files = VirtualHost.Assets.List(folderPath ?? "");
            if (files is null || files.Length == 0)
                return false;
            var fileName = Path.GetFileName(fullFilePath);
            foreach (var file in files)
                if (file == fileName)
                    return true;
            return false;
            */
            
        }
        catch
        {
            return false;
        }
        #elif BROWSERWASM
        throw new NotSupportedException($"{nameof(ProjectFileExists)} is not supported on this platform: {Environment.OSVersion.Platform}.");
        #else
        return File.Exists(fullFilePath);
        #endif
    }
    #endregion Internal


    #region Private

    private static readonly char[] DirectorySeparators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

    private static string[] SplitPathSegments(string? path)
    {
        return string.IsNullOrWhiteSpace(path)
            ? []
            : path.Split(DirectorySeparators, StringSplitOptions.RemoveEmptyEntries);
    }


    #endregion Private



}
