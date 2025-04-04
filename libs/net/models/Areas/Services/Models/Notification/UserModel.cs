using System.Text.Json;
using TNO.Core.Extensions;

namespace TNO.API.Areas.Services.Models.Notification;

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
    /// get/set - Unique key to identify the user.
    /// </summary>
    public string Key { get; set; } = "";

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
    /// get/set - Whether the user is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// get/set - Whether the user email is verified.
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// get/set - The user account type.
    /// </summary>
    public Entities.UserAccountType AccountType { get; set; }

    /// <summary>
    /// get/set - The user preferences.
    /// </summary>
    public JsonDocument Preferences { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// Get the value of the isVacationMode property from Preferences.
    /// </summary>
    /// <returns>Returns true if isVacationMode is set to true, otherwise false.</returns>
    public bool IsVacationMode()
    {
        if (Preferences.RootElement.TryGetProperty("isVacationMode", out JsonElement isVacationModeElement))
        {
            return isVacationModeElement.GetBoolean();
        }
        return false;
    }

    /// <summary>
    /// Get the value of the enableReportSentiment property from Preferences.
    /// </summary>
    /// <returns>Returns true if enableReportSentiment is set to true, otherwise false.</returns>
    public bool EnableReportSentiment()
    {
        if (Preferences.RootElement.TryGetProperty("enableReportSentiment", out JsonElement enableReportSentimentElement))
        {
            return enableReportSentimentElement.GetBoolean();
        }
        return false;
    }
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
    public UserModel(Entities.User entity)
    {
        this.Id = entity.Id;
        this.Key = entity.Key;
        this.Username = entity.Username;
        this.Email = entity.Email;
        this.PreferredEmail = entity.PreferredEmail;
        this.DisplayName = entity.DisplayName;
        this.FirstName = entity.FirstName;
        this.LastName = entity.LastName;
        this.IsEnabled = entity.IsEnabled;
        this.EmailVerified = entity.EmailVerified;
        this.AccountType = entity.AccountType;
        this.Preferences = entity.Preferences;
    }

    /// <summary>
    /// Creates a new instance of an UserModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public UserModel(API.Areas.Services.Models.Content.UserModel model)
    {
        this.Id = model.Id;
        this.Username = model.Username;
        this.Email = model.Email;
        this.PreferredEmail = model.PreferredEmail;
        this.DisplayName = model.DisplayName;
        this.FirstName = model.FirstName;
        this.LastName = model.LastName;
        this.AccountType = model.AccountType;
        this.Preferences = model.Preferences;
    }

    /// <summary>
    /// Creates a new instance of an UserModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public UserModel(API.Areas.Services.Models.User.UserModel model)
    {
        this.Id = model.Id;
        this.Username = model.Username;
        this.Email = model.Email;
        this.PreferredEmail = model.PreferredEmail;
        this.DisplayName = model.DisplayName;
        this.FirstName = model.FirstName;
        this.LastName = model.LastName;
        this.AccountType = model.AccountType;
        this.Preferences = model.Preferences;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Get the preferred email if it has been set.
    /// </summary>
    /// <returns></returns>
    public string GetEmail()
    {
        return String.IsNullOrWhiteSpace(this.PreferredEmail) ? this.Email : this.PreferredEmail;
    }

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
