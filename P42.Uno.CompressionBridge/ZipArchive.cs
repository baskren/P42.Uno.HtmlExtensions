using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace P42.Uno.CompressionBridge
{
    public class ZipArchive : System.IO.Compression.ZipArchive
    {
        public ZipArchive(Stream stream) : base(stream) { }

        public ZipArchive(Stream stream, ZipArchiveMode mode) : base(stream, (System.IO.Compression.ZipArchiveMode)mode) { }

        public ZipArchive(Stream stream, ZipArchiveMode mode, bool leaveOpen) : base(stream , (System.IO.Compression.ZipArchiveMode)mode, leaveOpen) { }

        public ZipArchive(Stream stream, ZipArchiveMode mode, bool leaveOpen, Encoding entryNameEncoding) : base(stream, (System.IO.Compression.ZipArchiveMode)mode, leaveOpen) { }

        public new ZipArchiveMode Mode  => (ZipArchiveMode)base.Mode; 


        public void ExtractToDirectory(string path)
        {
            System.IO.Compression.ZipFileExtensions.ExtractToDirectory(this, path);
        }
    }

}
