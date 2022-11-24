using System.Collections.Generic;

namespace TNO.CSS.Models;

public class UserModel
{
    #region Properties
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public Dictionary<string, string[]> Attributes { get; set; } = new Dictionary<string, string[]>();
    #endregion
}
