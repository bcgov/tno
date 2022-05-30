using TNO.Entities;

namespace TNO.API.Areas.Editor.Models.User;

/// <summary>
/// RegisterModel class, provides a model that represents an user's registration request.
/// </summary>
public class RegisterModel
{
    #region Properties
    /// <summary>
    /// get/set - User's email address.
    /// </summary>
    public string Email { get; set; } = "";

    /// <summary>
    /// get/set - Display name of user.
    /// </summary>
    public string Code { get; set; } = "";

    /// <summary>
    /// get/set - The current status of the user.
    /// </summary>
    public UserStatus Status { get; set; } = UserStatus.Preapproved;

    /// <summary>
    /// get/set - A message to send back to requester.
    /// </summary>
    public string Message { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a RegisterModel object.
    /// </summary>
    public RegisterModel() { }

    /// <summary>
    /// Creates a new instance of a RegisterModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="status"></param>
    /// <param name="message"></param>
    public RegisterModel(string email, UserStatus status, string message)
    {
        this.Email = email;
        this.Status = status;
        this.Message = message;
    }

    /// <summary>
    /// Creates a new instance of a RegisterModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="message"></param>
    public RegisterModel(string email, string message) : this(email, UserStatus.Denied, message)
    {
    }
    #endregion
}
