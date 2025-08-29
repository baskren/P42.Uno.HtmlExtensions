namespace P42.Uno;

public static class StreamExtensions
{
    public static byte[] ReadAllBytesFromStream(this Stream stream)
    {
        // Check if the stream is readable
        if (!stream.CanRead)
        {
            throw new NotSupportedException("The stream does not support reading.");
        }

        // If the stream supports seeking and has a known length,
        // we can pre-allocate the byte array for efficiency.
        if (stream.CanSeek && stream.Length > 0)
        {
            var buffer = new byte[stream.Length];
            int bytesRead; 
            var totalBytesRead = 0;

            while ((bytesRead = stream.Read(buffer, totalBytesRead, buffer.Length - totalBytesRead)) > 0)
            {
                totalBytesRead += bytesRead;
                // If for some reason the stream reports a length but then provides more data,
                // or if the length was initially incorrect, resize the buffer.
                if (totalBytesRead == buffer.Length && stream.CanSeek && stream.Position < stream.Length)
                {
                    Array.Resize(ref buffer, (int)stream.Length); 
                }
            }
            // If the stream's length was greater than the actual data read,
            // or if we pre-allocated based on length and didn't fill it,
            // resize to the actual bytes read.
            if (totalBytesRead < buffer.Length)
            {
                Array.Resize(ref buffer, totalBytesRead);
            }
            return buffer;
        }
        else // If the stream does not support seeking or has an unknown length
        {
            const int bufferSize = 4096; // Choose an appropriate buffer size
            var buffer = new byte[bufferSize];
            var allBytes = new List<byte>();
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (var i = 0; i < bytesRead; i++)
                {
                    allBytes.Add(buffer[i]);
                }
            }
            return allBytes.ToArray();
        }
    }
    
}
