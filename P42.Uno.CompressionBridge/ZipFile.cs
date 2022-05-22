using System;

namespace P42.Uno.CompressionBridge
{
    public static class ZipFile
    {
        public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName)
            => System.IO.Compression.ZipFile.ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName);


    }
}
