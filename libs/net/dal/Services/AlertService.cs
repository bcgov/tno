using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.DAL.Models;
using TNO.Entities;

namespace TNO.DAL.Services;


public class AlertService : BaseService<Alert, int>, IAlertService {

    #region Constructors
    public AlertService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<TagService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public Alert? FindAlert()
    {
        return this.Context.Alerts.FirstOrDefault();
    }
    #endregion

}