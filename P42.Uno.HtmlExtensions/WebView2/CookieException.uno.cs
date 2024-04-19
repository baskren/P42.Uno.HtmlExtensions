using System;
using System.Runtime.Serialization;

namespace P42.Net
{
    public class CookieException : FormatException, ISerializable
    {
        public CookieException() : base()
        {
        }

        internal CookieException(string message) : base(message)
        {
        }

        internal CookieException(string message, Exception inner) : base(message, inner)
        {
        }

        /*
        protected CookieException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            base.GetObjectData(serializationInfo, streamingContext);
        }

        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            base.GetObjectData(serializationInfo, streamingContext);
        }
        */
    }

}
