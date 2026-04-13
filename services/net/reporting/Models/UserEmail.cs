using MMI.SmtpEmail.Models;

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
    public UserEmail[] CC { get; set; } = [];

    /// <summary>
    /// get/set - An array of BCC to include with this email.
    /// </summary>
    public UserEmail[] BCC { get; set; } = [];

    /// <summary>
    /// get/set - Display name of user.
    /// </summary>
    public string DisplayName { get; set; } = "";

    /// <summary>
    /// get/set - First name of user.
    /// </summary>
    public string FirstName { get; set; } = "";

    /// <summary>
    /// get/set - Last name of user.
    /// </summary>
    public string LastName { get; set; } = "";
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
        this.CC = cc ?? [];
        this.BCC = bcc ?? [];
    }

    /// <summary>
    /// Creates a new instance of a UserEmail based on the specified user and email address to send to.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="to"></param>
    /// <param name="cc"></param>
    /// <param name="bcc"></param>
    public UserEmail(TNO.API.Areas.Services.Models.User.UserModel user, string to, UserEmail[]? cc = null, UserEmail[]? bcc = null)
    {
        this.UserId = user.Id;
        this.DisplayName = user.DisplayName;
        this.FirstName = user.FirstName;
        this.LastName = user.LastName;
        this.To = to;
        this.CC = cc ?? [];
        this.BCC = bcc ?? [];
    }

    /// <summary>
    /// Creates a new instance of a UserEmail based on the specified user and email address to send to.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="to"></param>
    /// <param name="cc"></param>
    /// <param name="bcc"></param>
    public UserEmail(API.Areas.Services.Models.Report.UserReportModel user, string to, UserEmail[]? cc = null, UserEmail[]? bcc = null)
    {
        this.UserId = user.UserId;
        this.DisplayName = user.User?.DisplayName ?? "";
        this.FirstName = user.User?.FirstName ?? "";
        this.LastName = user.User?.LastName ?? "";
        this.To = to;
        this.CC = cc ?? [];
        this.BCC = bcc ?? [];
    }


    /// <summary>
    /// Creates a new instance of a UserEmail based on the specified user and email address to send to.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="to"></param>
    /// <param name="cc"></param>
    /// <param name="bcc"></param>
    public UserEmail(API.Areas.Services.Models.AVOverview.UserModel user, string to, UserEmail[]? cc = null, UserEmail[]? bcc = null)
    {
        this.UserId = user.Id;
        this.DisplayName = user.DisplayName;
        this.FirstName = user.FirstName;
        this.LastName = user.LastName;
        this.To = to;
        this.CC = cc ?? [];
        this.BCC = bcc ?? [];
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

    /// <summary>
    /// Converts this UserEmail to a MailMergeModel, populating the context with the user's information. This allows for the generation of personalized email content using templates that can access user-specific data such as first name, last name, and user ID through the context. The resulting MailMergeModel can then be used to generate email content with dynamic values based on the user's information.
    /// </summary>
    /// <returns></returns>
    public MailMergeModel ToMailMergeModel()
    {
        return new MailMergeModel([this.To]).PopulateContextFromUser(new TNO.API.Areas.Services.Models.User.UserModel() { Id = this.UserId, FirstName = this.FirstName, LastName = this.LastName });
    }
    #endregion
}
