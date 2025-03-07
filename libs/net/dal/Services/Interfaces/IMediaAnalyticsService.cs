using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface IMediaAnalyticsService : IBaseService<MediaAnalytics, int>
{
    Task<IEnumerable<MediaAnalytics>> GetAllInformation();
    Task<MediaAnalytics> FilterBySourceId(int sourceid);
    Task<MediaAnalytics> FilterByMediaType(int mediatypeid);

    IPaged<MediaAnalytics> Find(MediaAnalyticsFilter filter);

}
