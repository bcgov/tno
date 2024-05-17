namespace TNO.API.Helpers
{
    /// <summary>
    /// IImpersonationHelper interface, provide helper methods for user authorization and impersonation.
    /// </summary>
    public interface IImpersonationHelper
    {
        /// <summary>
        /// Get the currently logged in user, or if they are impersonating get that user instead.
        /// </summary>
        /// <returns></returns>
        Entities.User GetCurrentUser();
    }
}
