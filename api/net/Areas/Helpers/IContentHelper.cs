namespace TNO.API.Helpers
{
    /// <summary>
    /// IContentHelper interface, provide helper methods for content.
    /// </summary>
    public interface IContentHelper
    {
        /// <summary>
        /// Generate a base 64 string for the image associated with the specified 'contentId'.
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        Task<string?> GetImageAsync(long contentId);
    }
}
