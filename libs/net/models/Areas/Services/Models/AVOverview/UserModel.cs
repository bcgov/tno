using System.Text.Json;
using TNO.Core.Extensions;

namespace TNO.API.Areas.Services.Models.AVOverview;

/// <summary>
/// UserModel class, provides a model that represents an user.
/// </summary>
public class UserModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - Unique username to identify user.
    /// </summary>
    public string Username { get; set; } = "";

    /// <summary>
    /// get/set - User's email address.
    /// </summary>
    public string Email { get; set; } = "";

    /// <summary>
    /// get/set - The user's preferred email address.
    /// </summary>
    public string PreferredEmail { get; set; } = "";

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

    /// <summary>
    /// get/set - The user account type.
    /// </summary>
    public Entities.UserAccountType AccountType { get; set; }

    /// <summary>
    /// get/set - The user preferences.
    /// </summary>
    public JsonDocument Preferences { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - Whether this user is subscribed to the report.
    /// </summary>
    public bool IsSubscribed { get; set; }

    /// <summary>
    /// get/set - How the email will be sent to the subscriber.
    /// </summary>
    public Entities.EmailSentTo SendTo { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an UserModel.
    /// </summary>
    public UserModel() { }

    /// <summary>
    /// Creates a new instance of an UserModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="isSubscribed"></param>
    /// <param name="sendTo"></param>
    public UserModel(Entities.User entity, bool isSubscribed, Entities.EmailSentTo sendTo = Entities.EmailSentTo.To)
    {
        this.Id = entity.Id;
        this.Username = entity.Username;
        this.Email = entity.Email;
        this.PreferredEmail = entity.PreferredEmail;
        this.DisplayName = entity.DisplayName;
        this.FirstName = entity.FirstName;
        this.LastName = entity.LastName;
        this.AccountType = entity.AccountType;
        this.Preferences = entity.Preferences;
        this.IsSubscribed = isSubscribed;
        this.SendTo = sendTo;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Get the preferred email if it has been set.
    /// </summary>
    /// <returns></returns>
    public string[] GetEmails()
    {
        if (this.AccountType == Entities.UserAccountType.Distribution)
        {
            var addresses = this.Preferences.GetElementValue<API.Models.UserEmailModel[]?>(".addresses");
            if (addresses != null) return addresses.Select(a => a.Email).ToArray();
            return Array.Empty<string>();
        }
        else
        {
            var email = String.IsNullOrWhiteSpace(this.PreferredEmail) ? this.Email : this.PreferredEmail;
            return new string[] { email };
        }
    }
    #endregion
}
