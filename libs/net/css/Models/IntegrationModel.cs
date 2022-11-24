using System;

namespace TNO.CSS.Models;

public class IntegrationModel
{
    #region Properties
    public int Id { get; set; }
    public string ProjectName { get; set; } = "";
    public string AuthType { get; set; } = "";
    public string Environment { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    #endregion
}
