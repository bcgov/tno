namespace TNO.CSS.Models;

public class UserRoleModel
{
    #region Properties
    public string Username { get; set; } = "";
    public string RoleName { get; set; } = "";
    public UserRoleOperation Operation { get; set; } = UserRoleOperation.Add;
    #endregion

    #region Constructors
    public UserRoleModel() { }

    public UserRoleModel(string username, string role, UserRoleOperation operation)
    {
        this.Username = username;
        this.RoleName = role;
        this.Operation = operation;
    }
    #endregion
}
