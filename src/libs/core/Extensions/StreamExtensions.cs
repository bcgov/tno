namespace TNO.Core.Extensions
{
    /// <summary>
    /// StreamExtensions static class, provides extension methods for Stream objects.
    /// </summary>
    public static class StreamExtensions
    {

        #region Methods
        /// <summary>
        /// Copy stream to byte array.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] ReadAllBytes(this Stream stream)
        {
            if (stream is MemoryStream)
                return ((MemoryStream)stream).ToArray();

            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
        #endregion
    }
}
