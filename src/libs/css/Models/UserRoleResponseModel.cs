using System;

namespace TNO.CSS.Models;

public class UserRoleResponseModel
{
    #region Properties
    public UserModel[] Users { get; set; } = Array.Empty<UserModel>();

    public RoleModel[] Roles { get; set; } = Array.Empty<RoleModel>();
    #endregion
}
