using System;
using System.Collections.Generic;
using System.Text;

namespace P42.Uno.CompressionBridge
{
    //
    // Summary:
    //     Specifies values for interacting with zip archive entries.
    public enum ZipArchiveMode
    {
        //
        // Summary:
        //     Only reading archive entries is permitted.
        Read = 0,
        //
        // Summary:
        //     Only creating new archive entries is permitted.
        Create = 1,
        //
        // Summary:
        //     Both read and write operations are permitted for archive entries.
        Update = 2
    }
}
