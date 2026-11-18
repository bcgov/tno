namespace TNO.CSS;

public class UserFilter
{
    #region Properties
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Guid { get; set; }
    public IdentityProvider Provider { get; set; }
    #endregion

    #region Constructors
    public UserFilter(IdentityProvider provider)
    {
        this.Provider = provider;
    }
    #endregion
}
