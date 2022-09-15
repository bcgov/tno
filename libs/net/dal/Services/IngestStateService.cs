using System.Security.Claims;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class IngestStateService : BaseService<IngestState, int>, IIngestStateService
{
    #region Properties
    #endregion

    #region Constructors
    public IngestStateService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<IngestStateService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods

    public IngestState AddOrUpdate(IngestState entity)
    {
        var existing = this.Context.IngestStates.Find(entity.IngestId);
        if (existing == null) return base.Add(entity);
        else
        {
            this.Context.Entry(existing).CurrentValues.SetValues(entity);
            return base.Update(existing);
        }
    }
    #endregion
}
