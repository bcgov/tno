namespace TNO.Keycloak;

public class UserFilter
{
    #region Properties
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool? EmailVerified { get; set; }
    public bool? Exact { get; set; }
    public bool? Enabled { get; set; }
    public string? Search { get; set; }
    #endregion
}
