using System;
using Windows.Storage;

namespace P42.Uno.HtmlExtensions;

/// <summary>
/// Result from ToPng and ToPdf processes
/// </summary>
public class ToFileResult
{
    /// <summary>
    /// Flags if the Result is an error;
    /// </summary>
    public bool IsError { get; private set; }

    /// <summary>
    /// Error message (or null)
    /// </summary>
    public string ErrorMessage
    {
        get;
        private set;
    }

    /// <summary>
    /// Windows.Storage.IStorageFile representation of PNG / PDF result file
    /// </summary>
    public IStorageFile StorageFile { get; private set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="storageFile"></param>
    public ToFileResult(IStorageFile storageFile)
    {
        StorageFile = storageFile;
    }

    /// <summary>
    /// Error constructor
    /// </summary>
    /// <param name="errorMessage"></param>
    public ToFileResult(string errorMessage)
    {
        IsError = true;
        ErrorMessage = errorMessage;
    }
}
