using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P42.Uno.HtmlExtensions;
internal static class FileSystem
{
    internal static DirectoryInfo AssureExists(string fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
            throw new ArgumentNullException(nameof(fullPath));
        DirectoryInfo info = Directory.Exists(fullPath)
            ? info = new DirectoryInfo(fullPath)
            : info = Directory.CreateDirectory(fullPath);
        if (!info.Exists)
            throw new Exception($"Could not assure existence of directory [{info}]");
        return info;
    }

}
