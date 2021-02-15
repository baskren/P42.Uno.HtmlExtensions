using System;
using Windows.Storage;

namespace P42.Uno.HtmlExtensions
{
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

        // Resutling output file (or null)
        public IStorageFile StorageFile { get; private set; }

        /// <summary>
        /// Html to PNG result
        /// </summary>
        /// <param name="isError"></param>
        /// <param name="errorMessage"></param>
		public ToFileResult(IStorageFile storageFile)
        {
            StorageFile = storageFile;
        }

        public ToFileResult(string errorMessage)
        {
            IsError = true;
            ErrorMessage = errorMessage;
        }
    }
}
