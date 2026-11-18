namespace TNO.CSS.Models;

public class RoleModel
{
    #region Properties
    public string Name { get; set; } = "";
    public bool Composite { get; set; }
    #endregion

    #region Constructors
    public RoleModel() { }

    public RoleModel(string name, bool composite = false)
    {
        this.Name = name;
        this.Composite = composite;
    }
    #endregion
}
