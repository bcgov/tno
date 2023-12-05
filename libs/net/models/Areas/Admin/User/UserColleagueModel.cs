using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.User;

/// <summary>
/// UserModel class, provides a model that represents an user.
/// </summary>
public class UserColleagueModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - User that relates to colleague.
    /// </summary>
    public UserModel? User { get; set; }

    /// <summary>
    /// get/set - Colleague.
    /// </summary>
    public UserModel? Colleague { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an UserModel.
    /// </summary>
    public UserColleagueModel() { }

    /// <summary>
    /// Creates a new instance of an UserModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="serializerOptions"></param>
    public UserColleagueModel(Entities.UserColleague entity, JsonSerializerOptions? serializerOptions = null) : base(entity)
    {
        this.User = new UserModel();
        this.Colleague = new UserModel();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a User object.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public Entities.UserColleague ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.UserColleague)this;
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.UserColleague(UserColleagueModel model)
    {
        var entity = new Entities.UserColleague(model.User.Id, model.Colleague.Id);
        return entity;
    }
    #endregion
}
