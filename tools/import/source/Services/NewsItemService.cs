using TNO.Tools.Import.Source.Entities;

namespace TNO.Tools.Import.Source.Services;

public class NewsItemService
{
  #region Properties
  private SourceContext context;
  #endregion

  #region Constructors
  public NewsItemService(SourceContext context)
  {
    this.context = context;
  }
  #endregion
}