using System.Text.Json.Serialization;

namespace TNO.API.Models;

/// <summary>
/// UserEmailModel class, provides a model that represents an user email pair.
/// </summary>
public class UserEmailModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to user.
    /// </summary>
    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    /// <summary>
    /// get/set - User's email address.
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an UserEmailModel.
    /// </summary>
    public UserEmailModel() { }

    /// <summary>
    /// Creates a new instance of an UserEmailModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="serializerOptions"></param>
    public UserEmailModel(Entities.User entity)
    {
        this.UserId = entity.Id;
        this.Email = String.IsNullOrWhiteSpace(entity.PreferredEmail) ? entity.Email : entity.PreferredEmail;
    }
    #endregion
}
