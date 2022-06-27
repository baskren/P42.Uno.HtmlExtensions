using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace P42.Uno.HtmlExtensions
{
    /// <summary>
    /// Provide WebView content from an EmbeddedResource
    /// </summary>
    public class EmbeddedSource
    {
        #region Static Implementation
        static EmbeddedSource()
        {
            var path = FolderPath(null);
            Directory.Delete(path, true);
        }

        static readonly object _locker = new object();
        static readonly Dictionary<string, Task<bool>> _cacheTasks = new Dictionary<string, Task<bool>>();

        // DO NOT CHANGE Environment.ApplicationDataPath to another path.  This is used to pass EmbeddedResource Fonts to UWP Text elements and there is zero flexibility here.
        /// <summary>
        /// Path to local cache location for Assembly's embedded resources
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public static string FolderPath(Assembly assembly, string folderName = null)
        {
            if (!Directory.Exists(ApplicationData.Current.LocalFolder.Path))
                Directory.CreateDirectory(ApplicationData.Current.LocalFolder.Path);
            var root = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, LocalStorageFolderName);
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
            if (assembly != null)
            {
                root = System.IO.Path.Combine(root, assembly.GetName().Name);
                if (!Directory.Exists(root))
                    Directory.CreateDirectory(root);
            }

            if (string.IsNullOrWhiteSpace(folderName))
                return root;

            var folderPath = System.IO.Path.Combine(root, folderName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            return folderPath;
        }

        /// <summary>
        /// Finds the folder for a embedded resource id
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        static string FindFolder(Assembly assembly, string resourceId)
        {
            var files = assembly.GetManifestResourceNames();
            var resourcePath = resourceId.Split('.');
            var folderPath = new List<string>();
            foreach (var file in files)
            {
                if (file != resourceId)
                {
                    var filePath = file.Split('.');
                    for (int i = folderPath.Count; i < resourcePath.Length - 2; i++)
                    {
                        if (resourcePath[i] == filePath[i])
                            folderPath.Add(filePath[i]);
                        else
                            break;
                    }
                    if (folderPath.Count == resourcePath.Length - 2)
                        break;
                }
            }
            return string.Join(".", folderPath);
        }

        /// <summary>
        /// Lists the contents of an embedded resource folder
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public static List<string> List(Assembly assembly, string folderName)
        {
            var folderPath = FolderPath(assembly, folderName);
            var files = Directory.EnumerateFiles(folderPath);
            return files.ToList();
        }

        /// <summary>
        /// Local cache path for an embedded resource
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="assembly"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public static async Task<string> LocalStorageFullPathForEmbeddedResourceAsync(string resourceId, Assembly assembly, string folderName = null)
        {
            if (await LocalStorageSubPathForEmbeddedResourceAsync(resourceId, assembly, folderName) is string subPath)
            {
                var path = System.IO.Path.Combine(FolderPath(assembly, folderName), subPath);
                return path;
            }
            return null;
        }

        public static async Task<string> LocalStorageSubPathForEmbeddedResourceAsync(string resourceId, Assembly assembly, string folderName = null)
        {
            if (assembly == null)
                return null;

            var fileName = resourceId;
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            var isZip = fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);

            if (fileName.StartsWith(folderName + ".", StringComparison.Ordinal))
                fileName = fileName.Substring((folderName + ".").Length);

            try
            {
                var path = isZip
                    ? System.IO.Path.Combine(FolderPath(assembly), resourceId)
                    : System.IO.Path.Combine(FolderPath(assembly, folderName), fileName);

                if (!_cacheTasks.ContainsKey(path) && (File.Exists(path) || Directory.Exists(path)))
                {
                    if (isZip)
                        return FolderPath(assembly, folderName);
                    return fileName;
                }

                if (CacheEmbeddedResource(resourceId, assembly, path) is Task<bool> task)
                {
                    if (await task)
                    {
                        if (isZip)
                        {
                            System.IO.Compression.ZipFile.ExtractToDirectory(path, FolderPath(assembly, folderName));
                            _cacheTasks.Remove(path);
                            return FolderPath(assembly, folderName);
                        }
                        _cacheTasks.Remove(path);
                        return fileName;
                    }
                    _cacheTasks.Remove(path);
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return null;
            }
        }

        static Task<bool> CacheEmbeddedResource(string resourceId, Assembly assembly, string fileName)
        {
            lock (_locker)
            {
                if (_cacheTasks.TryGetValue(fileName, out Task<bool> task))
                    return task;
                _cacheTasks.Add(fileName, task = CacheTask(resourceId, assembly, fileName));
                return task;
            }
        }

#pragma warning disable CS1998
        static async Task<bool> CacheTask(string resourceId, Assembly assembly, string path)
#pragma warning restore CS1998
        {
            try
            {
                using (var stream = assembly.GetManifestResourceStream(resourceId))
                {
                    if (stream is null)
                    {
                        Console.WriteLine("Cannot find EmbeddedResource [" + resourceId + "] in assembly [" + assembly.FullName + "].   Here are the ResourceIds in that assembly:");
                        foreach (var id in assembly.GetManifestResourceNames())
                            Console.WriteLine("\t" + id);
                        Console.WriteLine("");
                    }
                    else
                    {
                        //if (File.Exists(path))
                        //    System.Diagnostics.Debug.WriteLine("DownloadTask: FILE ALREADY EXISTS [" + path + "] [" + assembly.GetName().Name + ";" + resourceId + "]");

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            stream.CopyTo(fileStream);
                            fileStream.Flush(true);
                            var length = fileStream.Length;
                            //System.Diagnostics.Debug.WriteLine("DownloadTask: file written [" + path + "] [" + assembly.GetName().Name + ";" + resourceId + "] length=[" + length + "] name=[" + fileStream.Name + "] pos=[" + fileStream.Position + "]");
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            if (File.Exists(path))
                File.Delete(path);
            return false;
        }
        #endregion


        #region Instance Implementation
        public string Html { get; private set; }

        public string Path { get; private set; }

        public Assembly Assembly { get; private set; }
        public string FolderId { get; private set; }
        public string StartPageId { get; private set; }

        public EmbeddedSource(Assembly assembly, string startPageId = null)
        {
            if (assembly is null)
                throw new ArgumentException("Assembly cannot be null");
            Assembly = assembly;

            var ids = Assembly.GetManifestResourceNames();
            if (!ids.Any())
                throw new ArgumentException("There are no embedded resources in assembly [" + Assembly + "]");

            var folderId = FindFolder(assembly, startPageId);
            if (folderId?.EndsWith(".") ?? false)
                folderId.Trim('.');
            FolderId = folderId;

            if (!string.IsNullOrEmpty(folderId))
            {
                folderId += ".";
                if (startPageId?.StartsWith(folderId) ?? false)
                    startPageId = startPageId.Substring(folderId.Length);
            }
            if (string.IsNullOrWhiteSpace(startPageId))
            {
                foreach (var id in ids)
                {
                    if ((string.IsNullOrEmpty(folderId) || id.StartsWith(folderId))
                        && (id.EqualsWildcard("*.html") || id.EqualsWildcard("*.htm")))
                    {
                        if (string.IsNullOrWhiteSpace(startPageId))
                            startPageId = id.Substring(folderId.Length);
                        else
                            throw new ArgumentException("No startPageId given and there are multiple .html files in folder [" + Assembly + "][" + FolderId + "]");
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(startPageId))
                throw new ArgumentException("No startPageId given for folder [" + Assembly + "][" + FolderId + "]");
            StartPageId = startPageId;
        }

        public async Task Initialize()
        {
            var folderId = (string.IsNullOrEmpty(FolderId) ? null : FolderId + ".");
            var path = await LocalStorageFullPathForEmbeddedResourceAsync(folderId + StartPageId, Assembly, FolderId);
            var html = File.ReadAllText(path);
            var resourceNames = Assembly.GetManifestResourceNames();
            foreach (var resourceId in resourceNames)
            {
                if (resourceId.StartsWith(folderId, StringComparison.Ordinal))
                {
                    var resourcePath = resourceId.Split('.');
                    var suffix = resourcePath.LastOrDefault()?.ToLower();
                    if (suffix == "png"
                        || suffix == "jpg" || suffix == "jpeg"
                        || suffix == "svg"
                        || suffix == "gif"
                        || suffix == "tif" || suffix == "tiff"
                        || suffix == "pdf"
                        || suffix == "bmp"
                        || suffix == "ico")
                    {
                        var relativeSource = '"' + resourceId.Substring(folderId.Length) + '"';

                        if (html.Contains(relativeSource))
                        {
                            try
                            {
                                byte[] bytes;
                                using (var resourceStream = Assembly.GetManifestResourceStream(resourceId))
                                {
                                    using (var memoryStream = new MemoryStream())
                                    {
                                        resourceStream.CopyTo(memoryStream);
                                        bytes = memoryStream.ToArray();
                                    }
                                    var base64 = '"' + "data:" + (suffix == "pdf" ? "application/" : "image/") + suffix + ";base64," + Convert.ToBase64String(bytes) + '"';
                                    html = html.Replace(relativeSource, base64);
                                }
                            }
                            catch (Exception) { }
                        }
                    }
                }
            }
            Html = html;

        }

        const string LocalStorageFolderName = "P42.Uno.HtmlExtensions.EmbeddedResourceCache";

        #endregion
    }
}
