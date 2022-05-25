using System.Security.Claims;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class DataServiceService : BaseService<DataService, int>, IDataServiceService
{
    #region Properties
    #endregion

    #region Constructors
    public DataServiceService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<DataServiceService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods

    public DataService AddOrUpdate(DataService entity)
    {
        var existing = this.Context.DataServices.Find(entity.DataSourceId);
        if (existing == null) return base.Add(entity);
        else
        {
            this.Context.Entry(existing).CurrentValues.SetValues(entity);
            return base.Update(existing);
        }
    }
    #endregion
}
