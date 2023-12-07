using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.User;

/// <summary>
/// UserColleagueModel class, provides a model that represents a relatioship between a User and a Colleague.
/// </summary>
public class UserColleagueModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the colleague.
    /// </summary>
    public int ColleagueId { get; set; }

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
    /// Creates a new instance of an UserColleagueModel.
    /// </summary>
    public UserColleagueModel() { }

    /// <summary>
    /// Creates a new instance of an UserColleagueModel with primary keys.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="colleagueId"></param>
    public UserColleagueModel(int userId, int colleagueId) { 
        this.User = new UserModel(userId);
        this.Colleague = new UserModel(colleagueId);
    }

    /// <summary>
    /// Creates a new instance of an UserModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="serializerOptions"></param>
    public UserColleagueModel(Entities.UserColleague entity, JsonSerializerOptions? serializerOptions = null) : base(entity)
    {
        this.User = new UserModel
        {
            Id = entity.UserId
        };
        this.Colleague = new UserModel{
            Id = entity.ColleagueId,
            Username = entity.Colleague != null ? entity.Colleague.Username : "",
            Email = entity.Colleague != null ? entity.Colleague.Email : "",
        };
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
        var entity = new Entities.UserColleague(model.UserId, model.ColleagueId)
        {
            UserId = model.UserId,
            ColleagueId = model.ColleagueId,
        };
        return entity;
    }
    #endregion
}
