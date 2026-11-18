using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface IMediaAnalyticsService : IBaseService<MediaAnalytics, int>
{
    Task<IEnumerable<MediaAnalytics>> GetAllInformation();
    IPaged<MediaAnalytics> Find(MediaAnalyticsFilter filter);

}
