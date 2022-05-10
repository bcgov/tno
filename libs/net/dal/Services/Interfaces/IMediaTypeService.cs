using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

public interface IMediaTypeService : IBaseService<MediaType, int>
{
    IEnumerable<MediaType> FindAll();
    IPaged<MediaType> Find(MediaTypeFilter filter);
    MediaType? FindByName(string name);
}
