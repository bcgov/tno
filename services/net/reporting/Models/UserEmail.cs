namespace TNO.Services.Reporting.Models;

/// <summary>
/// UserEmail class, provides a model to identify users to send emails to.
/// </summary>
public class UserEmail
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to user.  Or 0 if not a user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// get/set - The email address.
    /// </summary>
    public string To { get; set; }

    /// <summary>
    /// get/set - An array of CC to include with this email.
    /// </summary>
    public UserEmail[] CC { get; set; } = Array.Empty<UserEmail>();

    /// <summary>
    /// get/set - An array of BCC to include with this email.
    /// </summary>
    public UserEmail[] BCC { get; set; } = Array.Empty<UserEmail>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserEmail.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="to"></param>
    /// <param name="cc"></param>
    /// <param name="bcc"></param>
    public UserEmail(int userId, string to, UserEmail[]? cc = null, UserEmail[]? bcc = null)
    {
        this.UserId = userId;
        this.To = to;
        this.CC = cc ?? Array.Empty<UserEmail>();
        this.BCC = bcc ?? Array.Empty<UserEmail>();
    }
    #endregion

    #region Methods
    public bool Equals(UserEmail? other)
    {
        if (other == null) return false;
        return this.To == other.To;
    }

    public override bool Equals(object? obj) => Equals(obj as UserEmail);
    public override int GetHashCode() => this.To.GetHashCode();
    #endregion
}
