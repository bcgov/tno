using System;

namespace TNO.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when content is modified by another user during update.
    /// </summary>
    public class ContentConflictException : Exception
    {
        /// <summary>
        /// Creates a new instance of a ContentConflictException object.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ContentConflictException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}