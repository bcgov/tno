using TNO.Tools.Import.Destination.Entities;

namespace TNO.Tools.Import.Destination.Services;

public class ContentService
{
    #region Properties
    private DestinationContext context;
    #endregion

    #region Constructors
    public ContentService(DestinationContext context)
    {
        this.context = context;
    }
    #endregion
}